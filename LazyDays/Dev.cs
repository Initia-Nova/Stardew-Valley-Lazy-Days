using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace LazyDays
{
    /// <summary>Patches used for development.</summary>
    internal sealed class Dev
    {
        /*********
        ** Public methods
        *********/

        /// <summary>The patch initialization method. Called by ModEntry.Entry.</summary>
        /// <param name="harmony">Provides the API for Harmony patches.</param>
        public static void Patch(Harmony harmony)
        {
            ModEntry.Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.OemPeriod)
            {
                ModEntry.Monitor.Log((Game1.player.currentLocation is Mine).ToString(), LogLevel.Debug);
                ModEntry.Monitor.Log((Game1.player.currentLocation is MineShaft).ToString(), LogLevel.Debug);
                ModEntry.Monitor.Log(Game1.player.currentLocation.GetDisplayName(), LogLevel.Debug);
            }
        }
    }
}
