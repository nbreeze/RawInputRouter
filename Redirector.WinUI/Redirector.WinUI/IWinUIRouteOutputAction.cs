using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.WinUI
{
    public interface IWinUIRouteOutputAction : IOutputAction
    {
        void Copy(IWinUIRouteOutputAction action);
    }
}
