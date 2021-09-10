using CommunityToolkit.Mvvm.ComponentModel;
using RawInputRouter.Imports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RawInputRouter.Routing
{
    public class ApplicationReceiver : ObservableObject, IApplicationReceiver
    {
        private string _Name = "";
        public virtual string Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string _ExecutableName = "";
        public virtual string ExecutableName { get => _ExecutableName; set => SetProperty(ref _ExecutableName, value); }

        private IntPtr _Handle = IntPtr.Zero;
        public virtual IntPtr Handle { get => _Handle; set => SetProperty(ref _Handle, value); }

        private Process _Process = null;
        public virtual Process Process { get => _Process; set => SetProperty(ref _Process, value); }

        public virtual IEnumerable<Process> FindProcesses()
        {
            if (string.IsNullOrEmpty(ExecutableName))
                return null;

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
                User32.EnumWindows(hWnd =>
                {
                    if (User32.IsWindow(hWnd) && User32.IsWindowVisible(hWnd))
                    {
                        int processId = 0;
                        User32.GetWindowThreadProcessId(hWnd, ref processId);
                        if (processIds.Contains(processId) && IsMatchingWindow(hWnd))
                        {
                            windows.Add(hWnd);
                        }
                    }

                    return true;
                });

                Handle = windows.FirstOrDefault();
                if (Handle != IntPtr.Zero)
                {
                    int processId = 0;
                    User32.GetWindowThreadProcessId(Handle, ref processId);
                    Process = Process.GetProcessById(processId);
                }
                else
                {
                    if (Process != null)
                    {
                        Process.Dispose();
                        Process = null;
                    }
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
