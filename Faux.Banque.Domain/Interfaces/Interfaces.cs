using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Interfaces
{
    ﻿public interface IApplicationService
     {
         void Execute(ICommand cmd);
     }

     public interface IEvent { }

     public interface ICommand { }

     public interface IIdentity { }
}
