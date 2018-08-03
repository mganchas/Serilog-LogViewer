using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveViewer.Utils
{
    public static class States
    {
        public enum StateTypes
        {
            Entry,
            Exit
        }

        public static StateTypes GetStateTypeFromString(string stateStr)
        {
            return stateStr.ToLower().Contains("enter") ? StateTypes.Entry : StateTypes.Exit;
        }
    }
}
