using System;
using HarmonyLib;
using RimWorld;
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
            if (GlobalState.ShouldKeepWeapon(___pawn, dead: false))
            {
                GlobalState.KeepWeapon.Add(___pawn);
            }
            if (GlobalState.ShouldKeepInventory(___pawn, dead: false))
            {
                GlobalState.KeepInventory.Add(___pawn);
            }
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] Pawn \"{GlobalState.PawnName(___pawn)}\" downed; marked: weapon={GlobalState.KeepWeapon.Contains(___pawn)}, inventory={GlobalState.KeepInventory.Contains(___pawn)}.");
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
            if (GlobalState.ShouldKeepWeapon(__instance, dead: true))
            {
                GlobalState.KeepWeapon.Add(__instance);
            }
            if (GlobalState.ShouldKeepInventory(__instance, dead: true))
            {
                GlobalState.KeepInventory.Add(__instance);
            }
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] Pawn \"{GlobalState.PawnName(__instance)}\" killed; marked: weapon={GlobalState.KeepWeapon.Contains(__instance)}, inventory={GlobalState.KeepInventory.Contains(__instance)}.");
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
            if (pawn != null && GlobalState.ShouldKeepWeapon(pawn, dead: false))
            {
                GlobalState.KeepWeapon.Add(pawn);
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

            if (!GlobalState.KeepWeapon.Contains(pawn))
            {
                if (GlobalState.Debug)
                {
                    DebugLog.Log($"[{Tag}] [X] Unmarked caller: pawn \"{GlobalState.PawnName(pawn)}\" drops its weapons (vanilla).");
                }
                return true;
            }

            GlobalState.KeepWeapon.Remove(pawn);
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its weapons.");
            }
            return false;
        }
    }

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

            if (!GlobalState.KeepInventory.Contains(pawn))
            {
                if (GlobalState.Debug)
                {
                    DebugLog.Log($"[{Tag}] [X] Unmarked caller: pawn \"{GlobalState.PawnName(pawn)}\" drops its inventory (vanilla).");
                }
                return true;
            }

            GlobalState.KeepInventory.Remove(pawn);
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its inventory.");
            }
            return false;
        }
    }

    /// <summary>
    /// public void Pawn_HealthTracker.CheckForStateChange(DamageInfo? dinfo, Hediff hediff).
    /// When a standing pawn loses its manipulation capacity (destroyed hands), vanilla
    /// drops the equipped weapon through TryDropEquipment directly - outside any of the
    /// drop-all methods. The prefix mirrors the exact branch conditions of that vanilla
    /// drop and marks the pawn so that single drop can be prevented. The vanilla
    /// branches for caravan members (weapon moves into inventory) and enclosed
    /// containers are deliberately left untouched.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    internal static class Patch_Pawn_HealthTracker_CheckForStateChange
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn ___pawn)
        {
            if (___pawn == null
                || ___pawn.Downed
                || ___pawn.kindDef.destroyGearOnDrop
                || ___pawn.InContainerEnclosed
                || !___pawn.SpawnedOrAnyParentSpawned
                || ___pawn.equipment?.Primary == null
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
            if (pawn is null || !GlobalState.KeepEquippedWeapon.Contains(pawn))
            {
                return true;
            }

            GlobalState.KeepEquippedWeapon.Remove(pawn);
            resultingEq = null;
            __result = true;
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] [-] Prevented: pawn \"{GlobalState.PawnName(pawn)}\" keeps its weapon despite losing manipulation.");
            }
            return false;
        }
    }

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
            if (!__result || !(item is Pawn takee))
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
