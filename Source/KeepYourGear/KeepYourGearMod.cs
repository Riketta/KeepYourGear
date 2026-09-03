using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace KeepYourGear
{
    public class KeepYourGearSettings : ModSettings
    {
        public bool keepColonistsWeapons = true;

        public bool keepColonistsInventory = true;

        public bool keepWeaponsAndInventoryOfDeadColonists = true;

        public bool keepOtherPawnsWeapons = true;

        public bool keepOtherPawnsInventory = true;

        public bool keepWeaponsAndInventoryOfOtherDeadPawns = true;

        public bool debugLogging = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref keepColonistsWeapons, "keepColonistsWeapons", true);
            Scribe_Values.Look(ref keepColonistsInventory, "keepColonistsInventory", true);
            Scribe_Values.Look(ref keepWeaponsAndInventoryOfDeadColonists, "keepWeaponsAndInventoryOfDeadColonists", true);
            Scribe_Values.Look(ref keepOtherPawnsWeapons, "keepOtherPawnsWeapons", true);
            Scribe_Values.Look(ref keepOtherPawnsInventory, "keepOtherPawnsInventory", true);
            Scribe_Values.Look(ref keepWeaponsAndInventoryOfOtherDeadPawns, "keepWeaponsAndInventoryOfOtherDeadPawns", true);
            Scribe_Values.Look(ref debugLogging, "debugLogging", false);
        }
    }

    public class KeepYourGearMod : Mod
    {
        /// <summary>Original package id, kept so the Steam Workshop item and existing
        /// installs keep their identity through the rename to Keep Your Gear.</summary>
        public const string PackageId = "Riketta.ReequipWeaponUponRecovery";

        /// <summary>Kept in sync with About/About.xml modVersion.</summary>
        public const string Version = "2.0";

        public static KeepYourGearSettings Settings;

        public KeepYourGearMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<KeepYourGearSettings>();
            // Patch each class separately: a game update that renames one target must
            // degrade to "that vanilla behavior stays", never break the other patch.
            Harmony harmony = new Harmony(PackageId);
            PatchSafe(harmony, typeof(Patch_Pawn_HealthTracker_MakeDowned));
            PatchSafe(harmony, typeof(Patch_Pawn_Kill));
            PatchSafe(harmony, typeof(Patch_Pawn_EquipmentTracker_Notify_PawnSpawned));
            PatchSafe(harmony, typeof(Patch_Pawn_EquipmentTracker_DropAllEquipment));
            PatchSafe(harmony, typeof(Patch_Pawn_InventoryTracker_DropAllNearPawn));
            PatchSafe(harmony, typeof(Patch_Pawn_HealthTracker_CheckForStateChange));
            PatchSafe(harmony, typeof(Patch_Pawn_EquipmentTracker_TryDropEquipment));
            PatchSafe(harmony, typeof(Patch_Pawn_CarryTracker_TryStartCarry));
            Log.Message("[KeepYourGear] v" + Version + " loaded (debugLogging="
                + Settings.debugLogging.ToString().ToLowerInvariant() + ").");
        }

        private static void PatchSafe(Harmony harmony, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                DebugLog.Log("applied " + patchClass.Name + ".");
            }
            catch (Exception e)
            {
                Log.Error("[KeepYourGear] Patch " + patchClass.Name + " could not be applied (game update?). " + e.Message);
            }
        }

        public override string SettingsCategory()
        {
            return "KeepYourGear.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("KeepYourGear.KeepColonistsWeapons".Translate(), ref Settings.keepColonistsWeapons, "KeepYourGear.KeepColonistsWeaponsTip".Translate());
            list.CheckboxLabeled("KeepYourGear.KeepColonistsInventory".Translate(), ref Settings.keepColonistsInventory, "KeepYourGear.KeepColonistsInventoryTip".Translate());
            list.Gap(12f);
            list.CheckboxLabeled("KeepYourGear.KeepWeaponsAndInventoryOfDeadColonists".Translate(), ref Settings.keepWeaponsAndInventoryOfDeadColonists, "KeepYourGear.KeepWeaponsAndInventoryOfDeadColonistsTip".Translate());
            list.Gap(12f);
            list.CheckboxLabeled("KeepYourGear.KeepOtherPawnsWeapons".Translate(), ref Settings.keepOtherPawnsWeapons, "KeepYourGear.KeepOtherPawnsWeaponsTip".Translate());
            list.CheckboxLabeled("KeepYourGear.KeepOtherPawnsInventory".Translate(), ref Settings.keepOtherPawnsInventory, "KeepYourGear.KeepOtherPawnsInventoryTip".Translate());
            list.CheckboxLabeled("KeepYourGear.KeepWeaponsAndInventoryOfOtherDeadPawns".Translate(), ref Settings.keepWeaponsAndInventoryOfOtherDeadPawns, "KeepYourGear.KeepWeaponsAndInventoryOfOtherDeadPawnsTip".Translate());
            if (Prefs.DevMode)
            {
                list.Gap(12f);
                list.CheckboxLabeled("KeepYourGear.DebugLogging".Translate(), ref Settings.debugLogging, "KeepYourGear.DebugLoggingTip".Translate());
            }
            list.End();
        }
    }
}
