using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cassandra;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.Storage;
namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class CassandraEventStoreTests
    {
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void CREATE_SESSION()
        {
            var cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();
            
            ISession session = cluster.Connect("EventStore");
            Assert.IsNotNull(session);
            cluster.Dispose();
            session.Dispose();

        }

        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void CREATE_STORE()
        {
            var cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();

            ISession session = cluster.Connect("EventStore");

            IAppendOnlyStore cassandraES = new CassandraEventStore(session);

            Assert.IsNotNull(cassandraES);

            cluster.Dispose();
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void INITIALIZE_STORE()
        {
            var cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();

            ISession session = cluster.Connect("EventStore");

            CassandraEventStore cassandraES = new CassandraEventStore(session);

            cassandraES.Initialize();

            cluster.Dispose();
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void GET_CURRENT_VERSION()
        {
            var cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();

            ISession session = cluster.Connect("EventStore");

            CassandraEventStore cassandraES = new CassandraEventStore(session);

            var ver = cassandraES.GetCurrentVersion();

            try
            {
                ver.Wait();
            }
            catch (Exception e)
            {

                GetExcepton(e);
            }
            

            cluster.Dispose();
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void READ_RECORDS_FROM_GIVEN_POINT_IN_TIME_MOVING_FORWARD()
        {
            var cluster = Cluster.Builder()
                 .AddContactPoints("localhost")
                     .Build();

            ISession session = cluster.Connect("EventStore");

            CassandraEventStore cassandraES = new CassandraEventStore(session);

            var records = cassandraES.ReadRecords(DateTimeOffset.MinValue, int.MaxValue);

            try
            {
                records.Wait();
            }
            catch (Exception e)
            {

                GetExcepton(e);
            }


            cluster.Dispose();
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void READ_ALL_RECORDS_FOR_GIVEN_KEY()
        {
            var cluster = Cluster.Builder()
                 .AddContactPoints("localhost")
                     .Build();

            ISession session = cluster.Connect("EventStore");

            CassandraEventStore cassandraES = new CassandraEventStore(session);

            var records = cassandraES.ReadRecords("account1",0, int.MaxValue);

            try
            {
                records.Wait();
            }
            catch (Exception e)
            {

                GetExcepton(e);
            }


            cluster.Dispose();
            session.Dispose();
        }
        private static string GetExcepton(Exception ex)
        {
            if (ex.InnerException == null) throw ex;
            return GetExcepton(ex.InnerException);
        }
    }
}
