using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows
{
    public abstract class Win32DeviceInput : DeviceInput
    {
        public abstract bool Matches(Win32DeviceInput input);
    }
}
