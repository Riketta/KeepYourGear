using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public bool Pawn_CarryTracker.TryStartCarry(Thing item).
    /// Vanilla strips a pawn at MakeDowned, so a pawn carried off by hostiles
    /// (kidnapping, hostage taking) has no gear on them. With this mod the gear stays
    /// on the pawn and would leave the map with the kidnapper, unrecoverable. The
    /// postfix makes a hostile carrier drop the pawn's gear at the pickup spot,
    /// restoring the vanilla outcome. Friendly carrying - rescue to bed, prisoner
    /// transport - is untouched.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_CarryTracker), "TryStartCarry")]
    internal static class Patch_Pawn_CarryTracker_TryStartCarry
    {
        private const string Tag = "Pawn_CarryTracker.TryStartCarry";

        [HarmonyPostfix]
        public static void Postfix(Pawn_CarryTracker __instance, Thing item, bool __result)
        {
            // Runs for every carried thing; the type check filters out ordinary item
            // hauling before anything else.
            if (!(item is Pawn takee) || !__result)
            {
                return;
            }
            Pawn carrier = __instance.pawn;
            if (carrier == null || !carrier.HostileTo(takee))
            {
                return;
            }

            try
            {
                if (!takee.SpawnedOrAnyParentSpawned)
                {
                    return;
                }
                IntVec3 pos = takee.PositionHeld;
                if (takee.equipment?.Primary != null)
                {
                    // rememberPrimary: true, like the vanilla downing drop, so a pawn
                    // rescued later still remembers and re-equips its weapon.
                    takee.equipment.DropAllEquipment(pos, forbid: true, rememberPrimary: true);
                }
                if (takee.inventory != null && takee.inventory.innerContainer.TotalStackCount > 0)
                {
                    takee.inventory.DropAllNearPawn(pos, forbid: true);
                }
                if (GlobalState.Debug)
                {
                    DebugLog.Log($"[{Tag}] Hostile \"{carrier.LabelShort}\" picked up \"{GlobalState.PawnName(takee)}\"; gear dropped at the pickup spot.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[KeepYourGear] [{Tag}] Exception: {ex}.");
            }
        }
    }
}
