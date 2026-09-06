using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// public void Pawn_GuestTracker.SetGuestStatus(Faction newHost, GuestStatus guestStatus).
    /// The vanilla prisoner branch calls Pawn.DropAndForbidEverything, which strips the pawn
    /// on capture - including colonists arrested in their own colony, for example to end a
    /// mental break. When the option is enabled, the prefix marks player-faction pawns made
    /// prisoners of the player faction for the duration of the call, so the existing drop
    /// prefixes prevent the strip and the colonist keeps weapon and inventory while
    /// imprisoned. Every other capture - enemies, slavery, quest transitions - stays vanilla.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    internal static class Patch_Pawn_GuestTracker_SetGuestStatus
    {
        private const string Tag = "Pawn_GuestTracker.SetGuestStatus";

        [HarmonyPrefix]
        public static void Prefix(Pawn ___pawn, Faction newHost, GuestStatus guestStatus)
        {
            KeepYourGearSettings settings = GlobalState.Settings;
            if (___pawn == null
                || settings == null
                || !settings.keepGearOfImprisonedColonists
                || guestStatus != GuestStatus.Prisoner
                || newHost != Faction.OfPlayer
                || !GlobalState.IsColonyMember(___pawn))
            {
                return;
            }
            GlobalState.KeepWeapon.Add(___pawn);
            GlobalState.KeepInventory.Add(___pawn);
            if (GlobalState.Debug)
            {
                DebugLog.Log($"[{Tag}] Pawn \"{GlobalState.PawnName(___pawn)}\" imprisoned; marked: weapon=true, inventory=true.");
            }
        }

        /// <summary>Runs even when SetGuestStatus throws, and preserves the exception.</summary>
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
}
