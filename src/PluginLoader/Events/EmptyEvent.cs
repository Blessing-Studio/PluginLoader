using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.PluginLoader.Events
{
    public class EmptyEvent : Event
    {
        public override string Name { get { return "EmptyEvent"; } }

        public override bool Do()
        {
            return true;
        }
    }
}
