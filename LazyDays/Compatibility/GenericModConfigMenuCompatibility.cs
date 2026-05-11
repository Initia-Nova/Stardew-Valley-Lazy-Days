namespace LazyDays.Compatibility
{
    internal sealed class GenericModConfigMenuCompatibility
    {
        public static void RegisterMenu()
        {
            // Get Generic Mod Config Menu's API if it's installed.
            var configMenu = ModEntry.Helper.ModRegistry.GetApi<
                GenericModConfigMenu.IGenericModConfigMenuApi
            >("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Register Lazy Days.
            configMenu.Register(
                mod: ModEntry.ModManifest,
                reset: ResetConfig,
                save: () => ModEntry.Helper.WriteConfig(ModEntry.Config)
            );

            // Add configuration options.
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                name: () => "Enable Lazy Days",
                getValue: () => ModEntry.Config.Enabled,
                setValue: SetEnabled
            );
            configMenu.AddKeybindList(
                mod: ModEntry.ModManifest,
                name: () => "Fast Forward Keys",
                getValue: () => ModEntry.Config.FastForwardKey,
                setValue: value => ModEntry.Config.FastForwardKey = value
            );
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                name: () => "Default Clock Rate",
                tooltip: () => "Real seconds per 10 game minutes.",
                getValue: () => ModEntry.Config.DefaultClockRate,
                setValue: value =>
                {
                    if (ModEntry.Config.DefaultClockRate == value) return;
                    ModEntry.Config.DefaultClockRate = value;
                    Clock.UpdateRate();
                },
                min: 1,
                max: 600
            );
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                name: () => "Fast Forward Clock Rate",
                tooltip: () => "Real seconds per 10 game minutes.",
                getValue: () => ModEntry.Config.FastForwardClockRate,
                setValue: value =>
                {
                    if (ModEntry.Config.FastForwardClockRate == value) return;
                    ModEntry.Config.FastForwardClockRate = value;
                    if (Clock.IsFastForward) Clock.UpdateRate();
                },
                min: 1,
                max: 600
            );
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                name: () => "Fishing Clock Rate",
                tooltip: () => "Only affects time when waiting for a fish to bite.\nReal seconds per 10 game minutes.",
                getValue: () => ModEntry.Config.FishingClockRate,
                setValue: value =>
                {
                    if (ModEntry.Config.FishingClockRate == value) return;
                    ModEntry.Config.FishingClockRate = value;
                    if (Clock.IsFishing) Clock.UpdateRate();
                },
                min: 1,
                max: 600
            );
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                name: () => "Pause Override",
                tooltip: () => "Stops the game from pausing on menus to help game time stay synced with real time.",
                getValue: () => ModEntry.Config.PauseOverride,
                setValue: value => ModEntry.Config.PauseOverride = value
            );
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                name: () => "Limit Mines Entry",
                tooltip: () => "Limits entry into the mines to once per day.",
                getValue: () => ModEntry.Config.LimitMinesEntry,
                setValue: value => ModEntry.Config.LimitMinesEntry = value
            );
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                name: () => "Limit Skull Cavern",
                tooltip: () => "Limits entry into Skull Cavern to once per day.",
                getValue: () => ModEntry.Config.LimitSkullCavernEntry,
                setValue: value => ModEntry.Config.LimitSkullCavernEntry = value
            );
        }

        public static void ResetConfig()
        {
            ModConfig oldConfig = ModEntry.Config;
            ModEntry.Config = new ModConfig
            {
                Dev = oldConfig.Dev
            };

            if (!oldConfig.Enabled)
            {
                ModEntry.Patch();
                return;
            }
            Clock.UpdateRate();
        }

        public static void SetEnabled(bool value)
        {
            if (ModEntry.Config.Enabled == value) return;
            ModEntry.Config.Enabled = value;
            if (value)
            {
                ModEntry.Patch();
            }
            else
            {
                ModEntry.Unpatch();
            }
        }
    }
}
