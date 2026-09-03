using HarmonyLib;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void Pawn_InventoryTracker.DropAllNearPawn(IntVec3 pos, bool forbid = false, bool unforbid = false).
    /// Prevented only for marked pawns - the drops initiated by MakeDowned and Kill.
    /// Pawn.Strip and CaravanEnterMapUtility.DropAllInventory never see a mark, so
    /// stripping and caravan (un)loading always work.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_InventoryTracker), "DropAllNearPawn")]
    internal static class Patch_Pawn_InventoryTracker_DropAllNearPawn
    {
        private const string Tag = "Pawn_InventoryTracker.DropAllNearPawn";

        [HarmonyPrefix]
        public static bool Prefix(Pawn_InventoryTracker __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn is null)
            {
                return true;
            }

            // HashSet.Remove returns whether the mark was present - consume it and let
            // only a marked drop be prevented.
            if (!GlobalState.KeepInventory.Remove(pawn))
            {
                if (GlobalState.Debug)
                {
                    DebugLog.Log($"[{Tag}] [X] Unmarked caller: pawn \"{GlobalState.PawnName(pawn)}\" drops its inventory (vanilla).");
                }
                return true;
            }

            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its inventory.");
            }
            return false;
        }
    }
}
