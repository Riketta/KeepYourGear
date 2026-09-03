using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void Pawn_EquipmentTracker.DropAllEquipment(IntVec3 pos, bool forbid = true, bool rememberPrimary = false).
    /// Prevented only for marked pawns - the drops initiated by MakeDowned, Kill and
    /// Notify_PawnSpawned. Pawn.Strip and every other caller never see a mark, so
    /// stripping (including strip orders on corpses) always works.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "DropAllEquipment")]
    internal static class Patch_Pawn_EquipmentTracker_DropAllEquipment
    {
        private const string Tag = "Pawn_EquipmentTracker.DropAllEquipment";

        [HarmonyPrefix]
        public static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn is null)
            {
                return true;
            }

            // DropAllEquipment drops item by item via TryDropEquipment; those per-item
            // drops must never be suppressed by the manipulation-loss mark.
            GlobalState.KeepEquippedWeapon.Remove(pawn);

            // HashSet.Remove returns whether the mark was present - consume it and let
            // only a marked drop be prevented.
            if (!GlobalState.KeepWeapon.Remove(pawn))
            {
                if (GlobalState.Debug)
                {
                    DebugLog.Log($"[{Tag}] [X] Unmarked caller: pawn \"{GlobalState.PawnName(pawn)}\" drops its weapons (vanilla).");
                }
                return true;
            }

            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its weapons.");
            }
            return false;
        }
    }
}
