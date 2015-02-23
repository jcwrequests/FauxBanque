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
        ICluster cluster;
        CassandraEnvironment environment;

        [TestInitialize]
        public void SetUp()
        {
            cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();

            environment = new CassandraEnvironment(cluster,"EventStore");
            
            
        }
        [TestCleanup]
        public void CleanUp()
        {
            cluster.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void CREATE_SESSION()
        {

            ISession session = environment.CreateSession();
            Assert.IsNotNull(session);
            session.Dispose();

        }

        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void CREATE_STORE()
        {
            ISession session = environment.CreateSession();

            IAppendOnlyStore cassandraES = new CassandraEventStore(session);

            Assert.IsNotNull(cassandraES);

            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void INITIALIZE_STORE()
        {
            ISession session = environment.CreateSession();
            
            CassandraEventStore cassandraES = new CassandraEventStore(session);
            environment.Initialize();
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void GET_CURRENT_VERSION()
        {
            ISession session = environment.CreateSession();

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
            
            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void READ_RECORDS_FROM_GIVEN_POINT_IN_TIME_MOVING_FORWARD()
        {
            ISession session = environment.CreateSession();

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

            session.Dispose();
        }
        [TestCategory("CASSANDRA_EVENT_STORE")]
        [TestMethod]
        public void READ_ALL_RECORDS_FOR_GIVEN_KEY()
        {
            ISession session = environment.CreateSession();

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

            session.Dispose();
        }
        private static string GetExcepton(Exception ex)
        {
            if (ex.InnerException == null) throw ex;
            return GetExcepton(ex.InnerException);
        }
    }
}
