using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redirector.Core
{
    public interface IApplicationReceiver
    {
        public string Name { get; set; }

        public string ExecutableName { get; set; }

        public void OnDelete();
    }
}