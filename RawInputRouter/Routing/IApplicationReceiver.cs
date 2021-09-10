using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RawInputRouter.Routing
{
    public interface IApplicationReceiver
    {
        public string ExecutableName { get; set; }

        public IntPtr Handle { get; set; }

        public string Name { get; set; }

        public Process Process { get; set; }

        public IEnumerable<Process> FindProcesses();

        public IntPtr FindWindow();

        public bool IsMatchingWindow(IntPtr handle);

        public void OnDelete();
    }
}