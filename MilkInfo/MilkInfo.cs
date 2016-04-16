using System;
using System.IO;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MilkInfo
{

    public class MilkInfo : Mod
    {
        private static readonly int offset = 10;
        private static readonly int boxOffset = 8;
        private static readonly int playEmote = 40;
        private static MilkInfoConfig ModConfig { get; set; }
        private static int updateInterval = 0;
        private static Dictionary<StardewValley.Object, int> machineRegister = new Dictionary<StardewValley.Object, int>();
        private static Texture2D pixel;

        public override void Entry(params object[] objects)
        {
            // Load config file (config.json).
            ModConfig = new MilkInfoConfig().InitializeConfig(BaseConfigPath);
            // Execute a handler when the save file is loaded.
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
            GraphicsEvents.OnPostRenderEvent += drawTickEvent;
            GraphicsEvents.OnPostRenderEvent += drawProgessBar;

        }

        private void drawProgessBar(object sender, EventArgs e)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            if (!Game1.hasLoadedGame) return;
            
            foreach (KeyValuePair<Vector2, StardewValley.Object> entry in Game1.currentLocation.objects)
            {
                StardewValley.Object tObj = entry.Value;
                if ( tObj.bigCraftable )
                {
                    bool keyPresent = machineRegister.ContainsKey(tObj);
                    if (tObj.minutesUntilReady > 0 && !keyPresent)
                    {
                        machineRegister.Add(tObj, tObj.minutesUntilReady);
                        Log.Debug(String.Format("Name: {0} Readyin: {1} Harvest: {2}", tObj.name, tObj.minutesUntilReady, tObj.readyForHarvest));
                    }
                    if (tObj.readyForHarvest && keyPresent)
                    {
                        machineRegister.Remove(tObj);
                    }
                
                }
            }
            foreach (KeyValuePair<StardewValley.Object, int> entry in machineRegister)
            {
                StardewValley.Object obj = entry.Key;
                Vector2 pos = obj.getLocalPosition(Game1.viewport);

                /* calculate progress */
                double progress = 100 - Math.Round((100 / (double)(entry.Value) * entry.Key.minutesUntilReady) * 2) / 2;

                /* surrounding box */
                Rectangle outerBox = new Rectangle((int)pos.X+4, (int)pos.Y +40 , obj.boundingBox.Width - offset, obj.boundingBox.Height / 2 - offset);

                /* actual progress bar box */
                int barWidth = (int)(Math.Max(((double)(obj.boundingBox.Width - offset) / 100) * progress, 1));
                Rectangle innerBox = new Rectangle((int)pos.X +8, (int)pos.Y +44, barWidth, obj.boundingBox.Height / 2 - offset - boxOffset);
                
                /* Draw Stuff*/
                Game1.spriteBatch.Draw(pixel, outerBox, Color.SaddleBrown);
                Game1.spriteBatch.Draw(pixel, innerBox, Color.GreenYellow);

            }
        }

        private void drawTickEvent(object sender, EventArgs e)
        {
            if (Game1.hasLoadedGame)
            {
                //Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                Texture2D fileTexture;

                using (FileStream fileStream = new FileStream(Path.Combine(PathOnDisk, @"Resources\herz.png"), FileMode.Open))
                {
                    fileTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fileStream);
                }

                Vector2 myposition = Game1.player.getLocalPosition(Game1.viewport);
                AnimatedSprite playerBox = Game1.player.sprite;

                Rectangle myrect = new Rectangle( (int) ( (myposition.X - 8)), (int) (myposition.Y - playerBox.spriteHeight * 6) , 80,80);

                Game1.spriteBatch.Draw(fileTexture, myrect, null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 1f);
                //Game1.spriteBatch.End();

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
    }
}