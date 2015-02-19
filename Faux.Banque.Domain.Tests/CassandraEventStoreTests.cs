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
    }
}
