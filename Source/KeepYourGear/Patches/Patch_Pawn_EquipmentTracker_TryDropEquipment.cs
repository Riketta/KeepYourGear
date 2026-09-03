using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public bool Pawn_EquipmentTracker.TryDropEquipment(ThingWithComps eq, out ThingWithComps resultingEq, IntVec3 pos, bool forbid = true).
    /// Suppressed only for the single manipulation-loss drop marked by
    /// Patch_Pawn_HealthTracker_CheckForStateChange. Every other caller - equipping a
    /// new weapon, stripping, hauling to inventory - is never marked and works normally.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
    internal static class Patch_Pawn_EquipmentTracker_TryDropEquipment
    {
        private const string Tag = "Pawn_EquipmentTracker.TryDropEquipment";

        [HarmonyPrefix]
        public static bool Prefix(Pawn_EquipmentTracker __instance, ref bool __result, ref ThingWithComps resultingEq)
        {
            Pawn pawn = __instance.pawn;
            if (pawn is null || !GlobalState.KeepEquippedWeapon.Remove(pawn))
            {
                return true;
            }

            resultingEq = null;
            __result = true;
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its weapon despite losing manipulation.");
            }
            return false;
        }
    }
}
