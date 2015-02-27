using Faux.Banque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Storage
{
    public interface IEventStore
    {
        EventStream LoadEventStream(IIdentity id);
        void AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events);
        IList<IEvent> LoadEvents(DateTimeOffset afterVersion, int maxCount);
        event EventStore.NewEventsArrivedHandler NewEventsArrived;
    }
}
