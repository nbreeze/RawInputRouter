using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using PInvoke;
using Redirector.Native;

namespace Redirector.Core
{
    public class DeviceSource : ObservableObject, IDeviceSource
    {
        private string _Name = "";
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string _Path = "";
        public string Path { get => _Path; set => SetProperty(ref _Path, value); }

        private bool _Block = false;
        public bool BlockOriginalInput { get => _Block; set => SetProperty(ref _Block, value); }

        public virtual bool ShouldBlockOriginalInput(DeviceInput input)
        {
            return BlockOriginalInput;
        }

        public virtual void OnDelete() 
        {
        }

        public virtual void OnInput(DeviceInput input)
        {
        }

        public virtual void OnConnect()
        {
        }

        public virtual void OnDisconnect()
        {
        }
    }
}
