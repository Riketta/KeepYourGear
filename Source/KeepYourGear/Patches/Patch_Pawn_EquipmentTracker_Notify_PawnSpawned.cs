using System;
using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void Pawn_EquipmentTracker.Notify_PawnSpawned().
    /// Vanilla drops all equipment of a pawn that spawns downed outside a bed: pawns
    /// placed into beds after being carried, pawns restored on save load and spawned
    /// quest pawns. The prefix marks the pawn so those drops are prevented like the
    /// MakeDowned ones.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PawnSpawned")]
    internal static class Patch_Pawn_EquipmentTracker_Notify_PawnSpawned
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = __instance.pawn;
            bool keepWeapon = pawn != null && GlobalState.ShouldKeepWeapon(pawn, dead: false);
            if (keepWeapon)
            {
                GlobalState.KeepWeapon.Add(pawn);
            }
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[Pawn_EquipmentTracker.Notify_PawnSpawned] Pawn \"{GlobalState.PawnName(pawn)}\" spawned; marked: weapon={keepWeapon}.");
            }
        }

        /// <summary>Runs even when Notify_PawnSpawned throws, and preserves the exception.</summary>
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = __instance?.pawn;
            if (pawn != null)
            {
                GlobalState.KeepWeapon.Remove(pawn);
            }
            return __exception;
        }
    }
}
