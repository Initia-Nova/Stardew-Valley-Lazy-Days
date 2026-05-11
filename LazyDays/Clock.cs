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

        /// <summary>The current clock rate.</summary>
        public static int CurrentRate { get; private set; }  = Game1.realMilliSecondsPerGameMinute / 100;

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

        /// <summary>The game clock rate before patches. Used when disabling the mod.</summary>
        private static int _prePatchRate = Game1.realMilliSecondsPerGameMinute / 100;

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
            _prePatchRate = Game1.realMilliSecondsPerGameMinute / 100;
            CurrentRate = _prePatchRate;
            ModEntry.Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            ModEntry.Helper.Events.Input.ButtonReleased += OnButtonReleased;
            UpdateRate();
        }

        /// <summary>Method called to reverse patches when the mod is disabled. Harmony patches are disabled in bulk.</summary>
        public static void Unpatch()
        {
            UpdateRate();
            ModEntry.Helper.Events.Input.ButtonsChanged -= OnButtonsChanged;
            ModEntry.Helper.Events.Input.ButtonReleased -= OnButtonReleased;
        }

        /// <summary>Updates the current clock rate.</summary>
        public static void UpdateRate()
        {
            int newRate = _prePatchRate;
            if (ModEntry.Config.Enabled)
            {
                newRate = ModEntry.Config.DefaultClockRate;
                if (IsFastForward && newRate > ModEntry.Config.FastForwardClockRate) newRate = ModEntry.Config.FastForwardClockRate;
                if (IsFishing && newRate > ModEntry.Config.FishingClockRate) newRate = ModEntry.Config.FishingClockRate;
            }

            if(ModEntry.Config.Dev) ModEntry.Monitor.Log($"Current Rate: {CurrentRate}. New Rate: {newRate}.", LogLevel.Debug);
            if (CurrentRate != newRate)
            {
                Game1.gameTimeInterval *= newRate;
                Game1.gameTimeInterval /= CurrentRate;
                Game1.realMilliSecondsPerGameMinute = 100 * newRate;
                Game1.realMilliSecondsPerGameTenMinutes = 1000 * newRate;
                RateChangedEventArgs eventArgs = new(CurrentRate, newRate);
                CurrentRate = newRate;
                RateChanged?.Invoke(typeof(ModEntry), eventArgs);
                if(ModEntry.Config.Dev) ModEntry.Monitor.Log($"Updating clock rate to {CurrentRate}.", LogLevel.Debug);
            }
        }

        /*********
        ** Private methods
        *********/
        
        /// <summary>Raised after the player presses or releases any buttons on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!IsFastForward && ModEntry.Config.FastForwardKey.JustPressed())
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
            if (IsFastForward && !ModEntry.Config.FastForwardKey.IsDown())
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
            if (!ModEntry.Config.PauseOverride) return;
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
                if(ModEntry.Config.Dev && !ignore) ModEntry.Monitor.Log(reason[..^1], LogLevel.Debug);
            }
            _pausing = 2;
            if (Game1.isFestival()) return;
            if (Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding) return;
            if (Game1.farmEvent != null) return;
            if (Game1.IsMultiplayer && !ignore_multiplayer) return;
            if (Game1.paused || Game1.freezeControls || Game1.isTimePaused) return;
            if (Game1.eventUp) return;
            if (ModEntry.Config.Dev && _pauseOverriden == 0 && !ignore) ModEntry.Monitor.Log("Skipping time freeze.", LogLevel.Debug);
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
