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

        public virtual void OnDelete() { }
    }
}
