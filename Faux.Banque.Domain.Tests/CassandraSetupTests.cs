using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cassandra;
using Faux.Banque.Domain.Storage;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class CassandraSetupTests
    {
        ICluster cluster;
        CassandraEnvironment environment;

        [TestInitialize]
        public void SetUp()
        {
            cluster = Cluster.Builder()
                .AddContactPoints("localhost")
                    .Build();

            environment = new CassandraEnvironment(cluster);
            try
            {
               environment.Drop();
            }
            catch
            {

            }
            
        }
        [TestCleanup]
        public void CleanUp()
        {
            cluster.Dispose();
        }
        [TestCategory("CASSANDRA_SETUP")]
        [TestMethod]
        public void TRUNCATE_TABLES()
        {
            ISession session = environment.CreateServerSession();
            CassandraEventStore cassandraES = new CassandraEventStore(session);
            environment.Initialize();
            environment.Truncate();

            session.Dispose();
        }
        [TestCategory("CASSANDRA_SETUP")]
        [TestMethod]
        public void INITIALIZE_STORE()
        {
            ISession session = environment.CreateServerSession();

            CassandraEventStore cassandraES = new CassandraEventStore(session);
            environment.Initialize();
            session.Dispose();
        }
    }
}
