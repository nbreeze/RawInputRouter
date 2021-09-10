using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RawInputRouter.Routing
{
    public class Route : ObservableObject, IRoute
    {
        public virtual IDeviceSource Source { get; set; }

        public virtual IApplicationReceiver Destination { get; set; }

        public virtual ObservableCollection<IOutputAction> Actions { get; } = new();

        public virtual IRouteInputFilter InputFilter { get; set; }

        private bool _BlockOriginalInput = false;

        public bool BlockOriginalInput { get => _BlockOriginalInput; set => SetProperty(ref _BlockOriginalInput, value); }

        public virtual void OnInput(IDeviceSource source, DeviceInput input)
        {
            if (InputFilter != null && !InputFilter.PassesFilter(this, source, input))
                return;

            foreach (IOutputAction action in Actions)
            {
                action.Dispatch(input, Source, Destination);
            }
        }

        public virtual bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input)
        {
            if (InputFilter != null && !InputFilter.PassesFilter(this, source, input))
                return false;

            return BlockOriginalInput;
        }
    }
}
