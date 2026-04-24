using HarmonyLib;
using StardewValley.Tools;

namespace LazyDays
{
    /// <summary>Patches for fishing.</summary>
    internal sealed class Fishing
    {
        /*********
        ** Public methods
        *********/

        /// <summary>The patch initialization method. Called by ModEntry.Entry.</summary>
        /// <param name="harmony">Provides the API for Harmony patches.</param>
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.DoFunction)),
                postfix: new HarmonyMethod(typeof(Fishing), nameof(FishingRodDoFunction_Postfix))
            );
        }

        /*********
        ** Internal methods
        *********/

        /// <summary>Harmony postfix patch for FishingRod.DoFunction to record when the player is fishing.</summary>
        /// <param name="__instance">The FishingRod object instance.</param>
        internal static void FishingRodDoFunction_Postfix(FishingRod __instance)
        {
            bool isFishing = __instance.isFishing && !__instance.isNibbling;
            if (isFishing != Clock.IsFishing)
            {
                Clock.IsFishing = isFishing;
                Clock.UpdateRate();
            }
            return;
        }
    }
}
