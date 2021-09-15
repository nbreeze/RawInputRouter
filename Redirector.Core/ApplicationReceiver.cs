using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PInvoke;

namespace Redirector.Core
{
    public class ApplicationReceiver : ObservableObject, IApplicationReceiver
    {
        private string _Name = "";
        public virtual string Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string _ExecutableName = "";
        public virtual string ExecutableName { get => _ExecutableName; set => SetProperty(ref _ExecutableName, value); }

        private IntPtr _Handle = IntPtr.Zero;
        public virtual IntPtr Handle { get => _Handle; set => SetProperty(ref _Handle, value); }

        private int _ProcessId = 0;
        public virtual int ProcessId { get => _ProcessId; set => SetProperty(ref _ProcessId, value); }

        public virtual IEnumerable<Process> FindProcesses()
        {
            if (string.IsNullOrEmpty(ExecutableName))
                return Array.Empty<Process>();

            string executableName = ExecutableName;
            string extension = Path.GetExtension(ExecutableName);
            if (!string.IsNullOrEmpty(extension) && extension.ToLower().Equals(".exe"))
            {
                executableName = Path.ChangeExtension(executableName, null);
            }

            return Process.GetProcessesByName(executableName);
        }

        public virtual IntPtr FindWindow()
        {
            IEnumerable<Process> processes = FindProcesses();

            try
            {
                if (!processes.Any())
                    return IntPtr.Zero;

                IEnumerable<int> processIds = processes.Select((a) => a.Id);

                List<IntPtr> windows = new List<IntPtr>();
                User32.EnumWindows((hWnd, lParam) =>
                {
                    if (User32.IsWindow(hWnd) && User32.IsWindowVisible(hWnd))
                    {
                        User32.GetWindowThreadProcessId(hWnd, out int processId);
                        if (processIds.Contains(processId) && IsMatchingWindow(hWnd))
                        {
                            windows.Add(hWnd);
                        }
                    }

                    return true;
                }, IntPtr.Zero);

                Handle = windows.FirstOrDefault();
                if (Handle != IntPtr.Zero)
                {
                    User32.GetWindowThreadProcessId(Handle, out int processId);
                    ProcessId = processId;
                }
                else
                {
                    ProcessId = 0;
                }
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }

            return Handle;
        }

        public virtual void OnDelete() { }

        public virtual bool IsMatchingWindow(IntPtr handle) => true;
    }
}
