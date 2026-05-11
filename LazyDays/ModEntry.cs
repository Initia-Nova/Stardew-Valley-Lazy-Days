using HarmonyLib;
using LazyDays.Compatibility;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace LazyDays
{
    /// <summary>The entry point for the LazyDays mod.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public fields
        *********/

        /// <summary>Provides manifest information for the LazyDays mod.</summary>
        public static new IManifest ModManifest { get; internal set; } = null!;

        /// <summary>Encapsulates monitoring and logging for the LazyDays mod.</summary>
        public static new IMonitor Monitor { get; internal set; } = null!;

        /// <summary>Provides simplified APIs for the LazyDays mod.</summary>
        public static new IModHelper Helper { get; internal set; } = null!;

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig Config { get; set; } = new ModConfig();

        /*********
        ** Private fields
        *********/

        /// <summary>The Harmony instance for patches.</summary>
        private static Harmony? _harmony = null;

        /// <summary>The ID used to initialize Harmony.</summary>
        private static string _harmonyID = "";

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Promote the ModManifest, Monitor, and Helper to static parameters for access in static methods.
            ModManifest = base.ModManifest;
            Monitor = base.Monitor;
            Helper = helper;

            // Read the config file.
            Config = Helper.ReadConfig<ModConfig>();

            // Listen for when the game launched event occurs to run compatibility code for other mods.
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Try to create a Harmony instance.
            _harmonyID = ModManifest.UniqueID;
            try
            {
                _harmony = new Harmony(_harmonyID);
            }
            catch (Exception e)
            {
                Monitor.Log($"Unable to launch a Harmony instance.\nException: {e.Message}.", LogLevel.Debug);
                _harmony = null;
            }

            if(Config.Enabled) Patch();
        }

        /// <summary>Apply all patches.</summary>
        public static void Patch()
        {
            if (_harmony != null)
            {
                if (Config.Dev) Dev.Patch(_harmony);
                Clock.Patch(_harmony);
                Fishing.Patch(_harmony);
                Mining.Patch(_harmony);
            }
        }

        /// <summary>Remove all patches.</summary>
        public static void Unpatch()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll(_harmonyID);
                if (Config.Dev) Dev.Unpatch();
                Clock.Unpatch();
                Mining.Unpatch();
            }
        }

        /// <summary>
        /// Raised after the game is launched, right before the first update tick.
        /// This happens once per game session (unrelated to loading saves).
        /// All mods are loaded and initialized at this point, so this is a good time to set up mod integrations.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        public static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GenericModConfigMenuCompatibility.RegisterMenu();
        }
    }
}
