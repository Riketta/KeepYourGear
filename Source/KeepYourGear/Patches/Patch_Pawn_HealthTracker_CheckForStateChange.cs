using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void Pawn_HealthTracker.CheckForStateChange(DamageInfo? dinfo, Hediff hediff).
    /// When a standing pawn loses its manipulation capacity (destroyed hands), vanilla
    /// drops the equipped weapon through TryDropEquipment directly - outside any of the
    /// drop-all methods. The prefix mirrors the exact branch conditions of that vanilla
    /// drop and marks the pawn so that single drop can be prevented. The vanilla
    /// branches for caravan members (weapon moves into inventory) and enclosed
    /// containers are deliberately left untouched.
    ///
    /// Runs on every health change of every pawn, so the guards are ordered
    /// cheapest/most selective first: most pawns (animals, weaponless) exit after the
    /// first field checks.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    internal static class Patch_Pawn_HealthTracker_CheckForStateChange
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn ___pawn)
        {
            if (___pawn == null
                || ___pawn.Downed
                || ___pawn.equipment?.Primary == null
                || ___pawn.kindDef.destroyGearOnDrop
                || ___pawn.InContainerEnclosed
                || !___pawn.SpawnedOrAnyParentSpawned
                || ___pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
                || !GlobalState.ShouldKeepWeapon(___pawn, dead: false))
            {
                return;
            }

            GlobalState.KeepEquippedWeapon.Add(___pawn);
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[Pawn_HealthTracker.CheckForStateChange] Pawn \"{GlobalState.PawnName(___pawn)}\" lost manipulation; weapon drop will be prevented.");
            }
        }

        /// <summary>Runs even when CheckForStateChange throws, and preserves the exception.</summary>
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, Pawn ___pawn)
        {
            if (___pawn != null)
            {
                GlobalState.KeepEquippedWeapon.Remove(___pawn);
            }
            return __exception;
        }
    }
}
