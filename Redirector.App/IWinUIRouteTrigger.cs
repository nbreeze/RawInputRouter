using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.App
{
    public interface IWinUIRouteTrigger : IRouteTrigger
    {
        void Copy(IWinUIRouteTrigger trigger);
    }
}
