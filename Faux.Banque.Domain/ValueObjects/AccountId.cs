using Faux.Banque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.ValueObjects
{
    public sealed class AccountId : IIdentity
    {

        public AccountId(string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            this.Value = value;
        }
        public readonly string Value;
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;
            var compare = obj as AccountId;
            if (compare == null) return false;
            return compare.Value.Equals(Value);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
    }
}
