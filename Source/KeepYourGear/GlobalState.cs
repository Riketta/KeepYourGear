using Verse;

namespace KeepYourGear
{
    internal static class GlobalState
    {
        public static KeepYourGearSettings ModSettings => KeepYourGearMod.Settings;

        /// <summary>Debug logging only counts as on when its setting is enabled and dev mode is active.</summary>
        public static bool Debug => (ModSettings?.debugLogging ?? false) && Prefs.DevMode;

        /// <summary>
        /// Mark that next DropAllEquipment call can be prevented if necessary.
        ///
        /// Valid call chains to prevent execution:
        /// "Pawn.DropAndForbidEverything -> Pawn_EquipmentTracker.DropAllEquipment";
        /// "Pawn_EquipmentTracker.Notify_PawnSpawned -> Pawn_EquipmentTracker.DropAllEquipment".
        ///
        /// Not valid (should be executed):
        /// "Pawn.Strip -> Pawn_EquipmentTracker.DropAllEquipment".
        /// </summary>
        public static bool CanSkipNextCallOfDropAllEquipment { get; set; }

        /// <summary>
        /// Mark that next DropAllNearPawn call can be prevented if necessary.
        ///
        /// Valid call chains to prevent execution:
        /// "Pawn.DropAndForbidEverything -> Pawn_InventoryTracker.DropAllNearPawn";
        ///
        /// Not valid (should be executed):
        /// "Pawn.Strip -> Pawn_InventoryTracker.DropAllNearPawn";
        /// "CaravanEnterMapUtility.DropAllInventory -> Pawn_InventoryTracker.DropAllNearPawn".
        /// </summary>
        public static bool CanSkipNextCallOfDropAllNearPawn { get; set; }
    }
}
