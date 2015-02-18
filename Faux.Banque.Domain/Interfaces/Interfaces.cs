using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faux.Banque.Domain.Interfaces
{
    ﻿public interface IApplicationService
     {
         void Execute(ICommand cmd);
     }

     public interface IEvent { }

     public interface ICommand { }

     public interface IIdentity { }

     public interface IAppendOnlyStore : IDisposable
     {
         void Append(string streamName, byte[] data, long expectedStreamVersion = -1);
         Task<IEnumerable<DataWithVersion>> ReadRecords(string streamName, long afterVersion, int maxCount);
         Task<IEnumerable<DataWithName>> ReadRecords(DateTimeOffset afterVersion, int maxCount);
         Task<DateTimeOffset> GetCurrentVersion();
     }

     public sealed class DataWithVersion
     {
         public readonly long Version;
         public readonly byte[] Data;

         public DataWithVersion(long version, byte[] data)
         {
             Version = version;
             Data = data;
         }
     }
     public sealed class DataWithName
     {
         public readonly string Name;
         public readonly byte[] Data;

         public DataWithName(string name, byte[] data)
         {
             Name = name;
             Data = data;
         }
     }
}
