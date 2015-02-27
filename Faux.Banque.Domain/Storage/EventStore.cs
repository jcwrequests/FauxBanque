using Faux.Banque.Domain.Exceptions;
using Faux.Banque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faux.Banque.Domain.Storage
{
    
    
    public class EventStore : IEventStore, IEventStoreSerializer
    {
        private IAppendOnlyStore appendOnlyStore;
        private IEventStoreSerializer serializer;
        public event NewEventsArrivedHandler NewEventsArrived;
        public delegate void NewEventsArrivedHandler(int count);

        public EventStore(IAppendOnlyStore appendOnlyStore, IEventStoreSerializer serializer)
        {
            if (appendOnlyStore == null) throw new ArgumentNullException("appendOnlyStore");
            if (serializer == null) throw new ArgumentNullException("serializer");

            this.appendOnlyStore = appendOnlyStore;
            this.serializer = serializer;
        }

        public EventStream LoadEventStream(IIdentity id)
        {
            var readTask = appendOnlyStore.ReadRecords(id.ToString(),0, int.MaxValue);
            readTask.Wait();
            EventStream stream = new EventStream();

            foreach (var record in readTask.Result)
            {
                stream.Events.AddRange(serializer.DeserializeEvent(record.Data));
                stream.Version = record.Version;
            }
            return stream;
                
        }

        public void AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events)
        {
            if (events.Count == 0)
                return;
            var name = id.ToString();
            var data = serializer.SerializeEvents(events.ToArray());
            try
            {
                appendOnlyStore.Append(name, data, expectedVersion);
                if (NewEventsArrived != null) NewEventsArrived(events.Count);
            }
            catch (AppendOnlyStoreConcurrencyException e)
            {
               
                var stream = LoadEventStream(id);
                throw OptimisticConcurrencyException.Create(stream.Version, expectedVersion, id.ToString(), stream.Events);
            }
        }

        


        public IList<IEvent> LoadEvents(DateTimeOffset afterVersion, int maxCount)
        {
            var readTask = this.appendOnlyStore.ReadRecords(afterVersion, maxCount);
            readTask.Wait();

            var events = new List<IEvent>();

            foreach (var record in readTask.Result)
            {
                events.AddRange(serializer.DeserializeEvent(record.Data));
            }
            return events;
        }
    }
}
