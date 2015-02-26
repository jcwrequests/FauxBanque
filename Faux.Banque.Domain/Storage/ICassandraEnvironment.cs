using System;
namespace Faux.Banque.Domain.Storage
{
    interface ICassandraEnvironment
    {
        Cassandra.ISession CreateEventStoreSession();
    }
}
