using HarmonyLib;
using StardewModdingAPI;

namespace LazyDays
{
    /// <summary>The entry point for the LazyDays mod.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public fields
        *********/

        /// <summary>Encapsulates monitoring and logging for the LazyDays mod.</summary>
        public static new IMonitor Monitor { get; internal set; } = null!;

        /// <summary>Provides simplified APIs for the LazyDays mod.</summary>
        public static new IModHelper Helper { get; internal set; } = null!;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Promote the Monitor and Helper to static parameters for access in static methods.
            Monitor = base.Monitor;
            Helper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            
            // Dev.Patch(harmony);
            Clock.Patch(harmony);
            Fishing.Patch(harmony);
            Mining.Patch(harmony);
        }
    }
}
