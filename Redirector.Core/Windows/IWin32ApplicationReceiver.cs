using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redirector.Core.Windows
{
    public interface IWin32ApplicationReceiver : IApplicationReceiver
    {
        IntPtr Handle { get; set; }
        int ProcessId { get; set; }

        IEnumerable<Process> FindProcesses();
        IntPtr FindWindow();
        bool IsMatchingWindow(IntPtr handle);
    }
}