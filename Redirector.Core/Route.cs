using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Redirector.Core
{
    public class Route : ObservableObject, IRoute
    {
        public virtual IDeviceSource Source { get; set; }

        public virtual IApplicationReceiver Destination { get; set; }

        public virtual ObservableCollection<IOutputAction> Actions { get; } = new();

        public virtual ObservableCollection<IRouteTrigger> Triggers { get; } = new();

        private bool _BlockOriginalInput = false;

        public bool BlockOriginalInput { get => _BlockOriginalInput; set => SetProperty(ref _BlockOriginalInput, value); }

        public Route() : base()
        {
            Triggers.CollectionChanged += OnTriggersCollectionChanged;
            Actions.CollectionChanged += OnActionsCollectionChanged;
        }

        private void OnActionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IOutputAction action in e.NewItems)
                    {
                        action.Route = this;
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (IOutputAction action in e.OldItems)
                    {
                        if (action.Route == this)
                            action.Route = null;
                    }
                    break;
            }
        }

        private void OnTriggersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IRouteTrigger trigger in e.NewItems)
                    {
                        trigger.Route = this;
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (IRouteTrigger trigger in e.OldItems)
                    {
                        if (trigger.Route == this)
                            trigger.Route = null;
                    }
                    break;
            }
        }

        public virtual bool ShouldTrigger(IDeviceSource source, DeviceInput input)
        {
            if (!input.CameFrom(Source))
                return false;

            foreach (IRouteTrigger trigger in Triggers)
            {
                if (trigger.ShouldTrigger(this, source, input))
                    return true;
            }

            return false;
        }

        public virtual void OnInput(IDeviceSource source, DeviceInput input)
        {
            if (!ShouldTrigger(source, input))
                return;

            foreach (IOutputAction action in Actions)
            {
                action.Dispatch(input, Source, Destination);
            }
        }

        public virtual bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input)
        {
            if (!ShouldTrigger(source, input))
                return false;

            return BlockOriginalInput;
        }
    }
}
