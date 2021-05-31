using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModTMNF.Mods
{
    public class Mod
    {
        public bool Applied { get; private set; }

        public void Apply()
        {
            if (!Applied)
            {
                OnApply();
                Applied = true;
            }
        }

        public void Remove()
        {
            if (Applied)
            {
                OnRemove();
                Applied = false;
            }
        }

        protected virtual void OnApply()
        {
        }

        protected virtual void OnRemove()
        {
        }
    }

    public static class ModManager
    {
        public static List<Mod> Mods = new List<Mod>();

        public static void Add(Mod mod)
        {
            Mods.Add(mod);
            mod.Apply();
        }

        public static void Remove(Mod mod)
        {
            mod.Remove();
            Mods.Remove(mod);
        }
    }
}
