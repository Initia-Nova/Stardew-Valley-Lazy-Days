using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;

namespace LazyDays
{
    /// <summary>Patches for the Mines.</summary>
    sealed class Mining
    {
        /*********
        ** Public fields
        *********/

        /// <summary>True if the player has already entered the Mines today.</summary>
        public static bool EnteredMinesToday = false;

        /*********
        ** Public methods
        *********/

        /// <summary>The patch initialization method. Called by ModEntry.Entry.</summary>
        /// <param name="harmony">Provides the API for Harmony patches.</param>
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(GameLocation), nameof(GameLocation.performAction), new Type[3] {typeof(string[]), typeof(Farmer), typeof(Location)}
                ),
                prefix: new HarmonyMethod(typeof(Mining), nameof(GameLocationPerformAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(Mining), nameof(Game1EnterMine_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
                prefix: new HarmonyMethod(typeof(Mining), nameof(MineShaftCheckAction_Prefix))
            );
            ModEntry.Helper.Events.GameLoop.DayStarted += OnDayStarted;
            ModEntry.Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        }

        /*********
        ** Private methods
        *********/
        
        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            EnteredMinesToday = false;
        }
        
        /// <summary>Raised once per second after the game state is updated.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(420) && Game1.player.currentLocation is MineShaft)
            {
                Game1.player.currentLocation.performTenMinuteUpdate(Game1.timeOfDay);
            }
        }

        /*********
        ** Internal methods
        *********/

        /// <summary>Harmony prefix patch for GameLocation.preformAction to prevent reentry into the Mines.</summary>
        /// <param name="__instance">The GameLocation object instance.</param>
        /// <param name="___registeredTileActions">The GameLocation.registeredTileActions field.</param>
        /// <param name="action">The action arguments to parse, excluding the Action prefix.</param>
        /// <param name="who">The player performing the action.</param>
        /// <param name="tileLocation">The tile coordinate of the action to handle.</param>
        /// <param name="__result">The returned result for the original method.</param>
        /// <returns>True if the original method should run.</returns>
        internal static bool GameLocationPerformAction_Prefix(
            GameLocation __instance,
            Dictionary<string, Func<GameLocation, string[], Farmer, Point, bool>> ___registeredTileActions,
            string[] action,
            Farmer who,
            Location tileLocation,
            ref bool __result
        )
        {
            // ModEntry.Monitor.Log($"Preforming action: {action.Join()}.", LogLevel.Debug);
            if (!who.IsLocalPlayer || __instance is not Mine || !EnteredMinesToday) return true;
            if (__instance.ShouldIgnoreAction(action, who, tileLocation)) return true;
            if (!ArgUtility.TryGet(action, 0, out var value, out var error, allowBlank: true, "string actionType")) return true;
            if (___registeredTileActions.TryGetValue(value, out var value2) == true) return true;

            switch (value)
            {
                case "MineElevator":
                case "Mine":
                    Game1.drawDialogueNoTyping(ModEntry.Helper.Translation.Get("EnteredMinesToday"));
                    __result = true;
                    return false;
            }

            return true;
        }

        /// <summary>Harmony postfix patch for Game1.enterMine to record when the player enters the Mines.</summary>
        /// <param name="whatLevel">The mine level.</param>
        internal static void Game1EnterMine_Postfix(int whatLevel)
        {
            if (whatLevel > 0 && whatLevel <= 120) EnteredMinesToday = true;
        }

        /// <summary>
        /// Harmony prefix patch for MineShaft.checkAction to prevent the player from moving between levels via the elevator in the Mines.
        /// </summary>
        /// <param name="__instance">The MineShaft object instance.</param>
        /// <param name="tileLocation">The tile coordinate of the action to handle.</param>
        /// <param name="who">The player performing the action.</param>
        /// <param name="__result">The returned result for the original method.</param>
        /// <returns>True if the original method should run.</returns>
        internal static bool MineShaftCheckAction_Prefix(MineShaft __instance, Location tileLocation, Farmer who, ref bool __result)
        {
            // ModEntry.Monitor.Log($"Checking action at {tileLocation.X}, {tileLocation.Y}.", LogLevel.Debug);
            if (who.IsLocalPlayer && __instance.getTileIndexAt(tileLocation, "Buildings", "mine") == 112 && __instance.mineLevel <= 120)
            {
                Response[] answerChoices = new Response[2]
                {
                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")).SetHotKey(Keys.Y),
                    new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
                };
                __instance.createQuestionDialogue(" ", answerChoices, "ExitMine");
                __result = true;
                return false;
            }

            return true;
        }
    }
}
