using Faux.Banque.Domain.Contracts;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Contacts
{
    public interface IAccount
    {
        CustomerId CustomerId { get; }
        AccountId AccountId { get; }
        decimal TotalBalance { get; }
        void Post(LedgerTransaction transaction);
    }

    
}
