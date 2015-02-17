using Faux.Banque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Faux.Banque.Domain.Exceptions;
using Cassandra.Data.Linq;
using Cassandra.Mapping;

namespace Faux.Banque.Domain.Storage
{

    public class Record
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Version { get; set; }
        public DateTimeOffset VersionTimeStamp { get; set; }
        public byte[] Data { get; set; }
    }
    public class RecordToBeProcesed
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Version { get; set; }
        public DateTimeOffset VersionTimeStamp { get; set; }
        public byte[] Data { get; set; }
        public bool Processed { get; set; }
    }
    public class EventStoreVersion
    {
        public DateTimeOffset VersionTimeStamp { get; set; }
        public bool Processed { get; set; }
    }
    public class CassandraEventStore : IAppendOnlyStore
    {

        private IMapper mapper;

        public CassandraEventStore(IMapper mapper)
        {
            if (mapper == null) throw new ArgumentNullException("mapper");
            this.mapper = mapper;

            MappingConfiguration.
                Global.
                Define(
                       new Map<Record>()
                          .TableName("Events").
                          PartitionKey("name", "version_time_stamp", "version").
                          ClusteringKey("version_time_stamp").
                          KeyspaceName("EventStore").
                          Column(r => r.Id, cm => cm.WithName("id").WithDbType<Guid>()).
                          Column(r => r.Name, cm => cm.WithName("name").WithDbType<string>()).
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Version, cm => cm.WithName("version").WithDbType<long>()).
                          Column(r => r.Data, cm => cm.WithName("data").WithDbType<byte[]>()),
                       new Map<RecordToBeProcesed>()
                          .TableName("Events").
                          PartitionKey("processed", "version_time_stamp", "name", "version").
                          ClusteringKey("version_time_stamp ASC", "name DESC", "version ASC").
                          KeyspaceName("EventStore").
                          Column(r => r.Id, cm => cm.WithName("id").WithDbType<Guid>()).
                          Column(r => r.Name, cm => cm.WithName("name").WithDbType<string>()).
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Version, cm => cm.WithName("version").WithDbType<long>()).
                          Column(r => r.Data, cm => cm.WithName("data").WithDbType<byte[]>()).
                          Column(r => r.Processed, cm => cm.WithName("processed").WithDbType<bool>()),
                       new Map<EventStoreVersion>().
                          TableName("EventsVersionsToBeProcessed").
                          PartitionKey("version_time_stamp","processed").
                          KeyspaceName("EventStore").
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Processed, cm => cm.WithName("processed").WithDbType<bool>()));




        }
        public void Initialize()
        {
            //using (var cluster = Cluster.Builder().AddContactPoint(connectionString).Build())
            //using (var session = cluster.Connect())
            //    {
            //    Dictionary<string,string> options = new Dictionary<string,string>();
            //    options.Add("class","SimpleStrategy");
            //    options.Add("replication_factor","3");

            //        session.CreateKeyspaceIfNotExists("EventStore", options);
            //        var table = session.GetTable<EventStoreRecord>("Events", "EventStore");
            //        table.CreateIfNotExists();

            //        //session.Execute("CREATE KEYSPACE IF NOT EXISTS EventStore WITH replication = {'class':'SimpleStrategy', 'replication_factor':3};");
            //        //session.Execute("CREATE TABLE EventStore.Events (" +
            //        //                        "id uuid," +
            //        //                        "name varchar" +
            //        //                        "version int," +
            //        //                        "version_time_stamp timestamp," +
            //        //                        "data blob," +
            //        //                        "PRIMARY KEY(id, name)" +
            //        //                        ");");

            //    }

        }
        public async void Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {

            var currentVersion = await mapper.
                FirstOrDefaultAsync<int>(string.Format("SELECT version FROM Events WHERE name = '{0}';",streamName));
            
            if (expectedStreamVersion >= 0)
            {
                if (currentVersion != expectedStreamVersion)
                {
                    throw new AppendOnlyStoreConcurrencyException(currentVersion, expectedStreamVersion, streamName);
                }
            }

            var batch = mapper.CreateBatch();

            Guid id = Guid.NewGuid();
            long version = expectedStreamVersion + 1;
            DateTimeOffset versionTimeStamp = DateTimeOffset.Now;

            batch.Insert<Record>(new Record
            {
                Id = id,
                Data = data,
                Name = streamName,
                Version = version,
                VersionTimeStamp = versionTimeStamp
            });
            batch.Insert<RecordToBeProcesed>(new RecordToBeProcesed
            {
                Id = id,
                Data = data,
                Name = streamName,
                Processed = false,
                Version = version,
                VersionTimeStamp = versionTimeStamp
            });
            batch.Insert<EventStoreVersion>(new EventStoreVersion 
            { 
                Processed = false, 
                VersionTimeStamp = versionTimeStamp 
            });

            mapper.Execute(batch);

        }


        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<DataWithVersion>> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            var results = await mapper.
                FetchAsync<Record>(string.Format("SELECT * FROM Events WHERE name = '{0}' LIMIT {1};",
                streamName, maxCount));

            return results.Select(r => new DataWithVersion(r.Version, r.Data));
            
        }

        public async Task<IEnumerable<DataWithName>> ReadRecords(DateTimeOffset afterVersion, int maxCount)
        {
            var result = await mapper.
                FetchAsync<Record>(string.Format("SELECT * FROM Events WHERE date_time_offset > {0} LIMIT {1} ALLOW FILTERING;",
                afterVersion, maxCount));

            return result.Select(r => new DataWithName(r.Name, r.Data));


        }

        public async Task<DateTimeOffset> GetCurrentVersion()
        {
            var version = await mapper.
                FirstOrDefaultAsync<EventStoreVersion>("SELECT * FROM EventsVersionsToBeProcessed LIMIT 1;");

            if (version == null) return DateTimeOffset.Now;

            return version.VersionTimeStamp;
        }
    }
}
