using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using RealFarmAnimals.I18n;


namespace RealFarmAnimals
{
    internal class ModEntry : Mod
    {
        public static IMonitor logger;
        private static Random random;
        private static ModConfig config;
        private IModHelper modHelper;
        private static int cropsEaten = 0;
        public override void Entry(IModHelper helper)
        {
            modHelper = helper;
            logger = Monitor;
            config = helper.ReadConfig<ModConfig>();

            i18n.Init(helper.Translation);

            Harmony harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            random = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);

            helper.Events.GameLoop.GameLaunched += SetupGMCM;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted; ;
        }

        private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
        {
            //
            //  notify the farmer of eaten crops
            //
            if (Game1.IsMasterGame)
            {
                if (cropsEaten > 0)
                {
                    if (config.SendMorningMessage)
                        Game1.addHUDMessage(new HUDMessage(i18n.CropsEaten(cropsEaten)));

                    cropsEaten = 0;
                }
            }
        }

        [HarmonyPatch(typeof(FarmAnimal))]
        [HarmonyPatch("updatePerTenMinutes")]
        class myPatch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                if (Game1.IsMasterGame)
                {
                    CheckForMunching(__instance);
                }
            }
        }

        private static void CheckForMunching(FarmAnimal animal)
        {
            if (!config.ModEnabled)
                return;

            Vector2 tileLocation = animal.Tile;
            switch (animal.facingDirection.Value)
            {
                case 0:
                    //up
                    tileLocation.Y = tileLocation.Y - 1;
                    break;
                case 1:
                    //right
                    tileLocation.X = tileLocation.X + 1;
                    break;
                case 2:
                    //down
                    tileLocation.Y = tileLocation.Y + 1;
                    break;
                case 3:
                    //left
                    tileLocation.X = tileLocation.X - 1;
                    break;
            }
            //
            //  if the animal has not eaten, look for crops
            //
            if (animal.fullness.Value < 128 && animal.currentLocation.Map.Layers[0].IsValidTileLocation((int)tileLocation.X, (int)tileLocation.Y))
            {
                if (animal.currentLocation.terrainFeatures.TryGetValue(tileLocation, out var t) && t is HoeDirt dirt && dirt.crop != null && dirt.readyForHarvest())
                {
                    //
                    //  roll the dice to see if the crop will be eaten
                    //
                    if (random.NextDouble() < config.Chance)
                    {
#if DEBUG
                        logger.Log($"Animal is eating", LogLevel.Debug);
#endif
                        if (!string.IsNullOrEmpty(config.EatingSound))
                            Game1.playSound(config.EatingSound);

                        cropsEaten++;
                        dirt.destroyCrop(true);
                        //
                        //  give same perk as eating grass
                        //
                        animal.fullness.Value = 255;
                        if (animal.moodMessage.Value != 5 && animal.moodMessage.Value != 6 && !animal.currentLocation.IsRainingHere())
                        {
                            animal.happiness.Value = 255;
                            animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + 8);
                        }
                    }
#if DEBUG
                    else
                    {
                        logger.Log($"Animal skipped eating crop", LogLevel.Debug);
                }
#endif
                }
            }
        }

        private void SetupGMCM(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = modHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => { config = new ModConfig(); },
                save: () => { modHelper.WriteConfig<ModConfig>(config); },
                 titleScreenOnly: false
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => i18n.Title(),
                tooltip: () => ""
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => i18n.Enabled(),
                tooltip: () => "",
                getValue: () => config.ModEnabled,
                setValue: value => config.ModEnabled = value
            );
            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => i18n.Message(),
               tooltip: () => i18n.Message_TT(),
               getValue: () => config.SendMorningMessage,
               setValue: value => config.SendMorningMessage = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => i18n.Chance(),
                tooltip: () => i18n.Chance_TT(),
                getValue: () => (int)(config.Chance * 1000),
                setValue: value => config.Chance = Math.Min(1000, Math.Max(0, value)) / 1000.0f
            );
            configMenu.AddTextOption(
               mod: ModManifest,
               name: () => i18n.Eating(),
               tooltip: () => i18n.Eating_TT(),
               getValue: () => config.EatingSound ?? "",
               setValue: value => config.EatingSound = value
            );
        }
    }
}
