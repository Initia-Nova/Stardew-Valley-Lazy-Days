using StardewModdingAPI.Utilities;

namespace LazyDays
{
    public sealed class ModConfig
    {
        /// <summary>Set to false to disable this mod.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Keybind for fast forwarding the clock.</summary>
        public KeybindList FastForwardKey { get; set; } = KeybindList.Parse("OemComma");

        /// <summary>The default clock rate in real seconds per ten game minutes.</summary>
        public int DefaultClockRate { get; set; } = 600;

        /// <summary>The clock rate while fast forwarding in real seconds per ten game minutes.</summary>
        public int FastForwardClockRate { get; set; } = 1;

        /// <summary>The clock rate while waiting on a fish to bite in real seconds per ten game minutes.</summary>
        public int FishingClockRate { get; set; } = 7;

        /// <summary>Set to false to use vanilla pause mechanics.</summary>
        public bool PauseOverride { get; set; } = true;

        /// <summary>Limit entry into the mines to once per day.</summary>
        public bool LimitMinesEntry { get; set; } = true;

        /// <summary>Limit entry into the skull cavern to once per day.</summary>
        public bool LimitSkullCavernEntry { get; set; } = true;

        /// <summary>Used during mod development. Should remain false.</summary>
        public bool Dev { get; set; } = false;
    }
}