using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Mods
{
    /// <summary>
    /// Trash class used to hook functions and dump stack traces when you're not sure of a given call chain.
    /// Put whatever hooks you want here to check stack traces, but remove it / comment it out after you're done.
    /// </summary>
    class ModStackTraceFinder : Mod
    {
        protected override void OnApply()
        {
            base.OnApply();
        }
    }
}
