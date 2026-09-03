using System;
using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// private void Pawn_HealthTracker.MakeDowned(DamageInfo? dinfo, Hediff hediff).
    /// For a pawn downed outside a bed this calls Pawn.DropAndForbidEverything, which
    /// drops the weapon and inventory. The prefix marks the pawn for the duration of
    /// the call so only these drops are candidates for prevention. Capture, trading and
    /// quest corpse generation call DropAndForbidEverything directly, without
    /// MakeDowned, and therefore keep their vanilla behavior.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    internal static class Patch_Pawn_HealthTracker_MakeDowned
    {
        private const string Tag = "Pawn_HealthTracker.MakeDowned";

        [HarmonyPrefix]
        public static void Prefix(Pawn ___pawn)
        {
            if (___pawn == null)
            {
                return;
            }
            bool keepWeapon = GlobalState.ShouldKeepWeapon(___pawn, dead: false);
            bool keepInventory = GlobalState.ShouldKeepInventory(___pawn, dead: false);
            if (keepWeapon)
            {
                GlobalState.KeepWeapon.Add(___pawn);
            }
            if (keepInventory)
            {
                GlobalState.KeepInventory.Add(___pawn);
            }
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] Pawn \"{GlobalState.PawnName(___pawn)}\" downed; marked: weapon={keepWeapon}, inventory={keepInventory}.");
            }
        }

        /// <summary>Runs even when MakeDowned throws, and preserves the exception.</summary>
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, Pawn ___pawn)
        {
            if (___pawn != null)
            {
                GlobalState.KeepWeapon.Remove(___pawn);
                GlobalState.KeepInventory.Remove(___pawn);
            }
            return __exception;
        }
    }
}
