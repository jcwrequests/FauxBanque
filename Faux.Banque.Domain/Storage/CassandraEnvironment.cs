using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Cassandra;

namespace Faux.Banque.Domain.Storage
{
   public class CassandraEnvironment
    {
      
       private ICluster cluster;
       
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
           if (cluster == null) throw new ArgumentNullException("cluster");
           this.cluster = cluster;
       }

       public  ISession CreateSession(string keySpace)
       {
           if (keySpace == null) throw new ArgumentNullException("keySpace");
           return cluster.Connect(keySpace);
       }
       
    }
}
