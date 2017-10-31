using System;
using StardewModdingAPI;

namespace MilkInfo
{
    class ModConfig
    {
        public int UpdateInterval { get; set; }

        public ModConfig()
        {
            this.UpdateInterval = 4;
        }
    }
}