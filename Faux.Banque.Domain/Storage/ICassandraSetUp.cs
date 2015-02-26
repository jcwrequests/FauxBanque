using System;
namespace Faux.Banque.Domain.Storage
{
    interface ICassandraSetUp
    {
        void Drop();
        void Initialize();
        void Truncate();
    }
}
