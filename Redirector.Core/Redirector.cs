using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace Redirector.Core
{
    public class Redirector : ObservableObject, IRedirector
    {
        public virtual ObservableCollection<IDeviceSource> Devices { get; } = new();

        public virtual ObservableCollection<IApplicationReceiver> Applications { get; } = new();

        public virtual ObservableCollection<IRoute> Routes { get; } = new();

        public Redirector() : base()
        {
            Devices.CollectionChanged += OnDevicesCollectionChanged;
            Applications.CollectionChanged += OnWindowsCollectionChanged;
        }

        public event EventHandler<RedirectorInputEventArgs> Input;

        public virtual void OnInput(IDeviceSource source, DeviceInput input)
        {
            source?.OnInput(input);

            DispatchInputToRoutes(source, input);

            Input?.Invoke(this, new(source, input));
        }

        protected void DispatchInputToRoutes(IDeviceSource source, DeviceInput input)
        {
            foreach (IRoute route in Routes)
            {
                route.OnInput(source, input);
            }
        }

        public virtual bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input)
        {
            if (source != null)
            {
                if (source.ShouldBlockOriginalInput(input))
                    return true;
            }

            foreach (IRoute route in Routes)
            {
                if (route.ShouldBlockOriginalInput(source, input))
                    return true;
            }

            return false;
        }

        protected virtual void OnWindowsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ApplicationReceiver window = (ApplicationReceiver)item;

                    if (window == null)
                        continue;

                    window.OnDelete();

                    foreach (var route in Routes)
                    {
                        if (route.Destination == window)
                        {
                            route.Destination = null;
                        }
                    }
                }
            }
        }

        protected virtual void OnDevicesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    IDeviceSource source = item as IDeviceSource;
                    if (source == null)
                        continue;

                    source.OnDelete();

                    foreach (var route in Routes)
                    {
                        if (route.Source == source)
                        {
                            route.Source = null;
                        }
                    }
                }
            }
        }
    }
}
