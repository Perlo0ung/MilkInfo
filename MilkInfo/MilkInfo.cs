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
        private static int emoteID = 0;


        public override void Entry(params object[] objects)
        {
            // Load config file (config.json).
            ModConfig = new MilkInfoConfig().InitializeConfig(BaseConfigPath);
            Command.RegisterCommand("emote", "Bla", new[] { "(Int32)<value> The target money" }).CommandFired += setEmoteID;
            // Execute a handler when the save file is loaded.
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
            GraphicsEvents.DrawTick += drawTickEvent;
        }

        // Draw event (when Map Page is opened)
        private void drawTickEvent(object sender, EventArgs e)
        {
            if (Game1.hasLoadedGame)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                Texture2D fileTexture;
                using (FileStream fileStream = new FileStream(@"C:\Users\Michael\Pictures\Items.png", FileMode.Open))
                {
                    fileTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fileStream);
                }

                Vector2 myposition = new Vector2(Game1.GlobalToLocal(Game1.player.position).X *Game1.options.zoomLevel ,Game1.GlobalToLocal(Game1.player.position).Y * Game1.options.zoomLevel +emoteID);
             
                Rectangle myrect = new Rectangle( (int) myposition.X, (int) myposition.Y, 3*16, 3*16);
                Game1.spriteBatch.Draw(fileTexture, myrect, Color.White);

                /*
                 Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);


                Game1.content.Load<Texture2D>("textures\\Milk");
                // Game1.spriteBatch.Draw(whitePixel, Game1.player.position , Color.SaddleBrown);

                Vector2 myposition = new Vector2(Game1.GlobalToLocal(Game1.player.position).X + -200, Game1.GlobalToLocal(Game1.player.position).Y -215);
                //sb.Draw(icon,Game1.GlobalToLocal(Game1.player.position),Color.Beige);
                Rectangle myrect = new Rectangle((int)myposition.X, (int)myposition.Y, 16, 16);

                Game1.spriteBatch.Draw(Game1.buffsIcons, myrect, Color.Red);

                Game1.spriteBatch.DrawString(Game1.smallFont, "!!!!!", myposition, null);

                Game1.spriteBatch.End();
             
                 * */

                Game1.spriteBatch.End();
            }
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