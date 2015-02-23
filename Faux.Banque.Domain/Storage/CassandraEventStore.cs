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
using CQL = Faux.Banque.Domain.Properties.Resources;
using System.Globalization;

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
        private ISession session;
        public CassandraEventStore(ISession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.mapper = new Mapper(session);
            this.session = session;

            

        }
        public void Initialize()
        {
            Dictionary<string,string> options = new Dictionary<string,string>();
            options.Add("class","SimpleStrategy");
            options.Add("replication_factor","3");

            session.CreateKeyspaceIfNotExists("EventStore", options);
            session.ChangeKeyspace("EventStore");

            Table<Record> record = new Table<Record>(session);
            record.CreateIfNotExists();
            Table<RecordToBeProcesed> recordTBP = new Table<RecordToBeProcesed>(session);
            recordTBP.CreateIfNotExists();
            Table<EventStoreVersion> version = new Table<EventStoreVersion>(session);
            version.CreateIfNotExists();
        }
        public async void Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            if (session.Keyspace != "EventStore") session.ChangeKeyspace("EventStore");

            var currentVersion = await mapper.
                FirstOrDefaultAsync<int>(string.Format(CQL.GetVersionForGivenKey, streamName));
            
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


        public void Dispose()
        {
            session.Dispose();
            mapper = null;
            session = null;
        }


        public async Task<IEnumerable<DataWithVersion>> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            if (session.Keyspace != "EventStore") session.ChangeKeyspace("EventStore");

            var results = await mapper.
                FetchAsync<Record>(string.Format(CQL.ReadRecordsByKey,
                streamName, maxCount));

            return results.Select(r => new DataWithVersion(r.Version, r.Data));
            
        }

        public async Task<IEnumerable<DataWithName>> ReadRecords(DateTimeOffset afterVersion, int maxCount)
        {
            if (session.Keyspace != "EventStore") session.ChangeKeyspace("EventStore");

            var result = await mapper.
                FetchAsync<Record>(string.Format(CQL.ReadRecordsByDataStamp,
                Encode(afterVersion), maxCount));

            return result.Select(r => new DataWithName(r.Name, r.Data));


        }

        public async Task<DateTimeOffset> GetCurrentVersion()
        {
            if (session.Keyspace != "EventStore") session.ChangeKeyspace("EventStore");

            var version = await mapper.
                FirstOrDefaultAsync<EventStoreVersion>(CQL.Version);

            if (version == null) return DateTimeOffset.Now;

            return version.VersionTimeStamp;
        }

        internal static readonly DateTimeOffset UnixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static string Encode(DateTimeOffset val)
        {
            if (val == DateTimeOffset.MinValue)
                return 0.ToString(CultureInfo.InvariantCulture);
            else
                return Convert.ToInt64(Math.Floor((val - UnixStart).TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
