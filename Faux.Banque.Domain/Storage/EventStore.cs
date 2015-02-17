using Faux.Banque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faux.Banque.Domain.Storage
{
    public class EventStore : IEventStore
    {
        public EventStream LoadEventStream(IIdentity id)
        {
            throw new NotImplementedException();
        }

        public void AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events)
        {
            throw new NotImplementedException();
        }
    }
}
