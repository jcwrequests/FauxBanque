using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Cassandra;
using CQL = Faux.Banque.Domain.Properties.Resources;


namespace Faux.Banque.Domain.Storage
{
   public class CassandraEnvironment
    {
      
       private ICluster cluster;
       private readonly string keySpace;

       /// <summary>
       /// Initializing the Mapping for the environment
       /// </summary>
       static CassandraEnvironment()
       {
           MappingConfiguration.
                Global.
                Define(
                       new Map<Record>()
                          .TableName("Events").
                          KeyspaceName("EventStore").
                          Column(r => r.Id, cm => cm.WithName("id").WithDbType<Guid>()).
                          Column(r => r.Name, cm => cm.WithName("name").WithDbType<string>()).
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Version, cm => cm.WithName("version").WithDbType<long>()).
                          Column(r => r.Data, cm => cm.WithName("data").WithDbType<byte[]>()).
                          PartitionKey(e => e.Name).
                          ClusteringKey(Tuple.Create("version_time_stamp", SortOrder.Ascending),
                                        Tuple.Create("version", SortOrder.Unspecified)),
                       new Map<RecordToBeProcesed>()
                          .TableName("Events").
                          KeyspaceName("EventStore").
                          PartitionKey("processed").
                          ClusteringKey(Tuple.Create("version_time_stamp", SortOrder.Ascending),
                                        Tuple.Create("name", SortOrder.Descending),
                                        Tuple.Create("version", SortOrder.Ascending)).
                          KeyspaceName("EventStore").
                          Column(r => r.Id, cm => cm.WithName("id").WithDbType<Guid>()).
                          Column(r => r.Name, cm => cm.WithName("name").WithDbType<string>()).
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Version, cm => cm.WithName("version").WithDbType<long>()).
                          Column(r => r.Data, cm => cm.WithName("data").WithDbType<byte[]>()).
                          Column(r => r.Processed, cm => cm.WithName("processed").WithDbType<bool>()),
                       new Map<EventStoreVersion>().
                          TableName("EventsVersionsToBeProcessed").
                          KeyspaceName("EventStore").
                          PartitionKey("version_time_stamp").
                          ClusteringKey("processed").
                          KeyspaceName("EventStore").
                          Column(r => r.VersionTimeStamp, cm => cm.WithName("version_time_stamp").WithDbType<DateTimeOffset>()).
                          Column(r => r.Processed, cm => cm.WithName("processed").WithDbType<bool>()));
       }
       public CassandraEnvironment(ICluster cluster) 
       {
           this.keySpace = "EventStore";
           if (cluster == null) throw new ArgumentNullException("cluster");
           if (keySpace == null) throw new ArgumentNullException("keySpace");

           this.cluster = cluster;
           
       }
       /// <summary>
       /// Create a session for the given keyspace
       /// </summary>
       /// <returns></returns>
       public  ISession CreateSession()
       {           
           return cluster.Connect(keySpace);
       }
       /// <summary>
       /// Initializes a Cassandra Cluster with a new Cassandra EventStore
       /// </summary>
       public void Initialize()
       {
            using (ISession session = cluster.Connect(keySpace))
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
            
       }
       /// <summary>
       /// Truncates all the data in the EventStore
       /// </summary>
       public void Truncate()
       {
           using (ISession session = cluster.Connect(keySpace))
           {
               session.Execute(CQL.Truncate);
           }
       }
       public void Drop()
       {

       }
       
    }
}
