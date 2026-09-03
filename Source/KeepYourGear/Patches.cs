using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false, bool rememberPrimary = false).
    /// Caller of Pawn_EquipmentTracker.DropAllEquipment and Pawn_InventoryTracker.DropAllNearPawn.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "DropAndForbidEverything")]
    internal static class Patch_Pawn_DropAndForbidEverything
    {
        private const string Tag = "Pawn.DropAndForbidEverything";

        [HarmonyPrefix]
        public static void Prefix(Pawn __instance)
        {
            DebugLog.Log($"[{Tag}] Pawn: \"{__instance.Name?.ToStringShort ?? __instance.ThingID}\"; Spawned: {__instance.Spawned}.");
            GlobalState.CanSkipNextCallOfDropAllEquipment = true;
            GlobalState.CanSkipNextCallOfDropAllNearPawn = true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            GlobalState.CanSkipNextCallOfDropAllEquipment = false;
            GlobalState.CanSkipNextCallOfDropAllNearPawn = false;
        }
    }

    /// <summary>
    /// public void DropAllEquipment(IntVec3 pos, bool forbid = true, bool rememberPrimary = false).
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

            try
            {
                string pawnName = pawn.Name is null ? pawn.ThingID : pawn.Name.ToStringShort;
                DebugLog.Log($"[{Tag}] > Pawn: \"{pawnName}\"; Weapon: \"{pawn.equipment?.Primary?.ToStringSafe()}\"; Player controlled: {pawn.IsPlayerControlled}; Faction: {pawn.Faction?.Name}; Dead: {pawn.Dead}.");

                // TODO: fix the case with weapons being dropped due to manipulation equal to zero (e.g. no hands).
                // TODO: fix the case with weapons being dropped due to death refusal.
                if (GlobalState.CanSkipNextCallOfDropAllEquipment)
                {
                    GlobalState.CanSkipNextCallOfDropAllEquipment = false;

                    // TODO: get rid of "pawn.IsPlayerControlled" and keep just faction?
                    bool isColonist = pawn.IsPlayerControlled && pawn.Faction == Faction.OfPlayer;

                    if (isColonist && GlobalState.ModSettings.keepColonistsWeapons && (!pawn.Dead || GlobalState.ModSettings.keepWeaponsAndInventoryOfDeadColonists))
                    {
                        DebugLog.Log($"[{Tag}] [-] Preventing original method execution! Colonist (\"{pawnName}\") will keep its weapon.");
                        return false;
                    }
                    if (!isColonist && GlobalState.ModSettings.keepOtherPawnsWeapons && (!pawn.Dead || GlobalState.ModSettings.keepWeaponsAndInventoryOfOtherDeadPawns))
                    {
                        DebugLog.Log($"[{Tag}] [-] Preventing original method execution! Non-player's pawn (\"{pawnName}\") will keep its weapon.");
                        return false;
                    }
                    DebugLog.Log($"[{Tag}] [+] Original method will be executed. Pawn (\"{pawnName}\") will drop its weapon.");
                }
                else
                {
                    DebugLog.Log($"[{Tag}] [X] Original undisturbed method will be executed. Pawn: \"{pawnName}\".");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[{Tag}] Exception: {ex}.");
            }

            return true;
        }
    }

    /// <summary>
    /// public void DropAllNearPawn(IntVec3 pos, bool forbid = false, bool unforbid = false).
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

            try
            {
                string pawnName = pawn.Name is null ? pawn.ThingID : pawn.Name.ToStringShort;
                DebugLog.Log($"[{Tag}] > Pawn: \"{pawnName}\"; Player controlled: {pawn.IsPlayerControlled}; Faction: {pawn.Faction?.Name}; Dead: {pawn.Dead}.");

                if (GlobalState.CanSkipNextCallOfDropAllNearPawn)
                {
                    GlobalState.CanSkipNextCallOfDropAllNearPawn = false;

                    // TODO: get rid of "pawn.IsPlayerControlled" and keep just faction?
                    bool isColonist = pawn.IsPlayerControlled && pawn.Faction == Faction.OfPlayer;

                    if (isColonist && GlobalState.ModSettings.keepColonistsInventory && (!pawn.Dead || GlobalState.ModSettings.keepWeaponsAndInventoryOfDeadColonists))
                    {
                        DebugLog.Log($"[{Tag}] [-] Preventing original method execution! Colonist (\"{pawnName}\") will keep its inventory.");
                        return false;
                    }
                    if (!isColonist && GlobalState.ModSettings.keepOtherPawnsInventory && (!pawn.Dead || GlobalState.ModSettings.keepWeaponsAndInventoryOfOtherDeadPawns))
                    {
                        DebugLog.Log($"[{Tag}] [-] Preventing original method execution! Non-player's pawn (\"{pawnName}\") will keep its inventory.");
                        return false;
                    }
                    DebugLog.Log($"[{Tag}] [+] Original method will be executed. Pawn (\"{pawnName}\") will drop its inventory items.");
                }
                else
                {
                    DebugLog.Log($"[{Tag}] [X] Original undisturbed method will be executed. Pawn: \"{pawnName}\".");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[{Tag}] Exception: {ex}.");
            }

            return true;
        }
    }

    /// <summary>
    /// public void Notify_PawnSpawned().
    /// Caller of Pawn_EquipmentTracker.DropAllEquipment.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PawnSpawned")]
    internal static class Patch_Pawn_EquipmentTracker_Notify_PawnSpawned
    {
        private const string Tag = "Pawn_EquipmentTracker.Notify_PawnSpawned";

        [HarmonyPrefix]
        public static void Prefix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn is null)
            {
                return;
            }

            DebugLog.Log($"[{Tag}] Pawn: \"{pawn.Name?.ToStringShort ?? pawn.ThingID}\"; Player controlled: {pawn.IsPlayerControlled}; Faction: {pawn.Faction?.Name}; Dead: {pawn.Dead}.");
            GlobalState.CanSkipNextCallOfDropAllEquipment = true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            GlobalState.CanSkipNextCallOfDropAllEquipment = false;
        }
    }
}
