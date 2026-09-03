using System;
using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public override void Pawn.Kill(DamageInfo? dinfo, Hediff exactCulprit = null).
    /// Calls Pawn.DropAndForbidEverything once the pawn is marked dead, dropping weapon
    /// and inventory next to the corpse. The prefix marks the pawn so the "dead" options
    /// apply to this drop and nowhere else.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    internal static class Patch_Pawn_Kill
    {
        private const string Tag = "Pawn.Kill";

        [HarmonyPrefix]
        public static void Prefix(Pawn __instance)
        {
            bool keepWeapon = GlobalState.ShouldKeepWeapon(__instance, dead: true);
            bool keepInventory = GlobalState.ShouldKeepInventory(__instance, dead: true);
            if (keepWeapon)
            {
                GlobalState.KeepWeapon.Add(__instance);
            }
            if (keepInventory)
            {
                GlobalState.KeepInventory.Add(__instance);
            }
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] Pawn \"{GlobalState.PawnName(__instance)}\" killed; marked: weapon={keepWeapon}, inventory={keepInventory}.");
            }
        }

        /// <summary>Runs even when Kill throws, and preserves the exception.</summary>
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, Pawn __instance)
        {
            if (__instance != null)
            {
                GlobalState.KeepWeapon.Remove(__instance);
                GlobalState.KeepInventory.Remove(__instance);
            }
            return __exception;
        }
    }
}
