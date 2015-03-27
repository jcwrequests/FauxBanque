using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Faux.Banque.Domain.Storage;
using System.Collections.Generic;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.Contacts;
using System.Linq;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestCategory("SERIALIZATION")]
        [TestMethod]
        public void SERIALIEZE_EVENTS()
        {
            JSONEventStoreSerializer seriliazer;
            seriliazer = new JSONEventStoreSerializer();
            List<IEvent> events = new List<IEvent>();
            events.Add(new CustomerCreated("John", "Smith", new ValueObjects.CustomerId("ID")));

            var results = seriliazer.SerializeEvents(events.ToArray());

            Assert.IsTrue(results.Length != 0);

        }
        [TestCategory("SERIALIZATION")]
        [TestMethod]
        public void DESERIALIEZE_EVENTS()
        {
            JSONEventStoreSerializer seriliazer;
            seriliazer = new JSONEventStoreSerializer();
            List<IEvent> events = new List<IEvent>();
            events.Add(new CustomerCreated("John", "Smith", new ValueObjects.CustomerId("ID")));

            var results = seriliazer.SerializeEvents(events.ToArray());

            var deserializedEvents = seriliazer.DeserializeEvent(results);

            Assert.IsTrue(deserializedEvents.Count != 0);
        }
    }
}
