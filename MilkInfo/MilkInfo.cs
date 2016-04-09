using System;
using System.IO;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MilkInfo
{

    public class MilkInfo : Mod
    {
        private static MilkInfoConfig ModConfig { get; set; }
        private static int updateInterval = 0;
        private static int playEmote = 40;
        private static int emoteID = 1;


        public override void Entry(params object[] objects)
        {
            // Load config file (config.json).
            ModConfig = new MilkInfoConfig().InitializeConfig(BaseConfigPath);
            Command.RegisterCommand("emote", "Bla", new[] { "(Int32)<value> The target money" }).CommandFired += setEmoteID;
            // Execute a handler when the save file is loaded.
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame; ;
            GraphicsEvents.DrawTick += drawTickEvent;
        }


        // Draw event (when Map Page is opened)
        private void drawTickEvent(object sender, EventArgs e)
        {
            if (Game1.hasLoadedGame)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                
                Texture2D fileTexture;

                using (FileStream fileStream = new FileStream(Path.Combine(PathOnDisk, @"Resources\herz.png"), FileMode.Open))
                {
                    fileTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fileStream);
                }


                Vector2 myposition = Game1.player.getLocalPosition(Game1.viewport);
                AnimatedSprite playerBox = Game1.player.sprite;

                Rectangle myrect = new Rectangle( (int) ( (myposition.X - 8) * Game1.options.zoomLevel), (int) ( (myposition.Y - playerBox.spriteHeight * 6 )  
                    * Game1.options.zoomLevel), (int) (80 * Game1.options.zoomLevel), (int)(80 * Game1.options.zoomLevel));

                Game1.spriteBatch.Draw(fileTexture, myrect, null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 1f);

                Game1.spriteBatch.End();
            }
        }
        
        private static int getGameZoom()
        {
            return Convert.ToInt32(Math.Ceiling(Game1.options.zoomLevel));
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
                    if (animal.currentProduce != -1 && !animal.isBaby())
                    {
                        /* melkbar und kein baby*/
                        animal.doEmote(playEmote);
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