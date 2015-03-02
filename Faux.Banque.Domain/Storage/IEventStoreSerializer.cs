using System;
namespace Faux.Banque.Domain.Storage
{
    public interface IEventStoreSerializer
    {
        System.Collections.Generic.List<Faux.Banque.Domain.Interfaces.IEvent> DeserializeEvent(byte[] data);
        byte[] SerializeEvents(Faux.Banque.Domain.Interfaces.IEvent[] events);
    }
}
