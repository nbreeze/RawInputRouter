﻿using Redirector.Core.Windows.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.App.Actions
{
    public class WinUIAcceleratorOutputAction : AcceleratorOutputAction, IWinUIRouteOutputAction
    {
        public virtual void Copy(IWinUIRouteOutputAction action)
        {
            Accelerator = (action as WinUIAcceleratorOutputAction).Accelerator;
        }
    }
}