using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redirector.Core
{
    public interface IApplicationReceiver
    {
        public string ExecutableName { get; set; }

        public IntPtr Handle { get; set; }

        public string Name { get; set; }

        public int ProcessId { get; set; }

        public IEnumerable<Process> FindProcesses();

        public IntPtr FindWindow();

        public bool IsMatchingWindow(IntPtr handle);

        public void OnDelete();
    }
}