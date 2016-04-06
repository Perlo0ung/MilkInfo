using StardewModdingAPI;

namespace MilkInfo
{
    public class MilkInfoConfig : Config
    {
        public int UpdateInterval { get; set; }
        public override T GenerateDefaultConfig<T>()
        {
            UpdateInterval = 4;
            return this as T;
        }
    }
}