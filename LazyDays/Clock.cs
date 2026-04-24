using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LazyDays
{
    /// <summary>Patches for the in-game clock.</summary>
    sealed class Clock
    {
        /*********
        ** Public fields
        *********/

        /// <summary>The default clock rate to match real time.</summary>
        public const int REAL_TIME_SECONDS_PER_TEN_MINUTES = 600;

        /// <summary>The clock rate for fast forwarding.</summary>
        public const int FAST_FORWARD_SECONDS_PER_TEN_MINUTES = 1;

        /// <summary>The clock rate for fishing.</summary>
        public const int FISHING_SECONDS_PER_TEN_MINUTES = 7;

        /// <summary>The current clock rate.</summary>
        public static int CurrentRate { get; private set; } = REAL_TIME_SECONDS_PER_TEN_MINUTES;

        /// <summary>The event handler for when the clock rate changes.</summary>
        public static event EventHandler<RateChangedEventArgs>? RateChanged;

        /// <summary>True if the player is activating fast forward.</summary>
        public static bool IsFastForward = false;

        /// <summary>True if the player is fishing.</summary>
        public static bool IsFishing = false;

        /*********
        ** Private fields
        *********/

        /// <summary>Decrementing counter used to log each pause event only once.</summary>
        private static int _pausing = 0;

        /// <summary>Decrementing counter used to log each pause event overriden only once.</summary>
        private static int _pauseOverriden = 0;

        /*********
        ** Public methods
        *********/

        /// <summary>The patch initialization method. Called by ModEntry.Entry.</summary>
        /// <param name="harmony">Provides the API for Harmony patches.</param>
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.shouldTimePass)),
                postfix: new HarmonyMethod(typeof(Clock), nameof(Game1ShouldTimePass_Postfix))
            );
            Game1.realMilliSecondsPerGameMinute = 100 * CurrentRate;
            Game1.realMilliSecondsPerGameTenMinutes = 1000 * CurrentRate;
            ModEntry.Helper.Events.Input.ButtonPressed += OnButtonPressed;
            ModEntry.Helper.Events.Input.ButtonReleased += OnButtonReleased;
        }

        /// <summary>Updates the current clock rate.</summary>
        public static void UpdateRate()
        {
            int newRate = REAL_TIME_SECONDS_PER_TEN_MINUTES;
            if (IsFastForward) newRate = FAST_FORWARD_SECONDS_PER_TEN_MINUTES;
            else if (IsFishing) newRate = FISHING_SECONDS_PER_TEN_MINUTES;

            if (CurrentRate != newRate)
            {
                Game1.gameTimeInterval *= newRate;
                Game1.gameTimeInterval /= CurrentRate;
                Game1.realMilliSecondsPerGameMinute = 100 * newRate;
                Game1.realMilliSecondsPerGameTenMinutes = 1000 * newRate;
                RateChangedEventArgs eventArgs = new(CurrentRate, newRate);
                CurrentRate = newRate;
                RateChanged?.Invoke(typeof(ModEntry), eventArgs);
                // ModEntry.Monitor.Log($"Updating clock rate to {CurrentRate}.", LogLevel.Debug);
            }
        }

        /*********
        ** Private methods
        *********/
        
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!IsFastForward && e.Button == SButton.OemComma)
            {
                IsFastForward = true;
                UpdateRate();
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (IsFastForward && e.Button == SButton.OemComma)
            {
                IsFastForward = false;
                UpdateRate();
            }
        }

        /*********
        ** Internal methods
        *********/

        /// <summary>Harmony postfix patch for Game1.shouldTimePass to prevent unnecessary clock pauses.</summary>
        /// <param name="ignore_multiplayer">True if time should not pause in multiplayer outside of festivals and events.</param>
        /// <param name="__result">The returned result for the original method.</param>
        internal static void Game1ShouldTimePass_Postfix(bool ignore_multiplayer, ref bool __result)
        {
            _pausing = Math.Max(_pausing - 1, 0);
            _pauseOverriden = Math.Max(_pauseOverriden - 1, 0);
            if (__result) return;
            bool ignore = true;
            if (_pausing == 0)
            {
                string reason = "Freezing time:";
                if (Game1.isFestival()) {
                    reason += " isFestival,";
                    ignore = false;
                }
                if (Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding) {
                    reason += " isWedding,";
                    ignore = false;
                }
                if (Game1.farmEvent != null) {
                    reason += " farmEvent,";
                    ignore = false;
                }
                if (Game1.IsMultiplayer && !ignore_multiplayer) {
                    reason += " IsMultiplayer & IsTimePaused,";
                    ignore = false;
                }
                if (Game1.paused) {
                    reason += " paused,";
                    ignore = false;
                }
                if (Game1.freezeControls) {
                    reason += " freezeControls,";
                    ignore = false;
                }
                if (Game1.overlayMenu != null) {
                    reason += " overlayMenu,";
                    ignore = false;
                }
                if (Game1.isTimePaused) {
                    reason += " isTimePaused,";
                    ignore = false;
                }
                if (Game1.eventUp) {
                    reason += " eventUp,";
                    ignore = false;
                }
                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is not BobberBar) reason += " activeClickableMenu,";
                if (!Game1.player.CanMove && !Game1.player.UsingTool) reason += " !CanMove & !UsingTool,";
                if (!ignore) ModEntry.Monitor.Log(reason[..^1], LogLevel.Debug);
            }
            _pausing = 2;
            if (Game1.isFestival()) return;
            if (Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding) return;
            if (Game1.farmEvent != null) return;
            if (Game1.IsMultiplayer && !ignore_multiplayer) return;
            if (Game1.paused || Game1.freezeControls || Game1.isTimePaused) return;
            if (Game1.eventUp) return;
            if (_pauseOverriden == 0 && !ignore) ModEntry.Monitor.Log("Skipping time freeze.", LogLevel.Debug);
            _pauseOverriden = 2;
            __result = true;
            return;
        }

        /*********
        ** Public Classes
        *********/

        /// <summary>Event arguments when the clock rate is changed.</summary>
        public class RateChangedEventArgs : EventArgs
        {
            /*********
            ** Public fields
            *********/

            /// <summary>The previous clock rate.</summary>
            public int OldRate {get; private set;}

            /// <summary>The current clock rate.</summary>
            public int NewRate {get; private set;}

            /// <summary>Default constructor.</summary>
            /// <param name="oldRate">The previous clock rate.</param>
            /// <param name="newRate">The current clock rate.</param>
            public RateChangedEventArgs(int oldRate, int newRate)
            {
                OldRate = oldRate;
                NewRate = newRate;
            }
        }
        
    }
}
