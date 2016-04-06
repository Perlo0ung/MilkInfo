using System;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;

namespace MilkInfo
{

    public class MilkInfo : Mod
    {
        private static MilkInfoConfig ModConfig { get; set; }
        private static int updateInterval = 0;
        private static int emoteID = 40;

        public override void Entry(params object[] objects)
        {
            // Load config file (config.json).
            ModConfig = new MilkInfoConfig().InitializeConfig(BaseConfigPath);
            Command.RegisterCommand("emote", "Bla", new[] { "(Int32)<value> The target money" }).CommandFired += setEmoteID;
            // Execute a handler when the save file is loaded.
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
        }

        private static void PlayerEvents_LoadedGame(object sender, EventArgsLoadedGameChanged e)
        {
            // Only load the event handler after the save file has been loaded.
            GameEvents.OneSecondTick += TimeEvents_ShowMilkIcon;
        }

    
        protected static void TimeEvents_ShowMilkIcon(object sender, EventArgs e)
        {
            // The logic in this handler will be executed once every <ModConfig.UpdateInterval> seconds.
            updateInterval++;
            if (updateInterval < ModConfig.UpdateInterval)
            {
                return;
            }
            updateInterval = 0;
            ModConfig.ReloadConfig();

            // Only check for animal status if the player is somewhere in the farm.
            var currentLocation = Game1.currentLocation;
            if (currentLocation == null || !currentLocation.isFarm)
            {
                Log.Error("Failed to get correct Location");
                return;
            }

            var animals = Game1.getFarm().getAllFarmAnimals();
            if (animals == null)
            {
                Log.Error("Failed to retrieve farm animals.");
                return;
            }

            foreach (FarmAnimal animal in animals)
            {
                string animalType = animal.type.ToLower();

                if (animalType.Contains("goat") || animalType.Contains("cow") || animalType.Contains("sheep"))
                {
                    if (animal.currentProduce != -1 )
                    {
                        /* melkbar*/
                        animal.doEmote(emoteID);
                    }                    
                }
            }

        }

        private static void setEmoteID(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                var ou = 0;
                if (int.TryParse(e.Command.CalledArgs[0], out ou))
                {
                    emoteID = ou;
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

    }
}