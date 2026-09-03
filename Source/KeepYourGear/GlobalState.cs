using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KeepYourGear
{
    /// <summary>
    /// Central state shared by the Harmony patches.
    ///
    /// All marks are pawn-keyed so that a leaked entry (an exception between a mark and
    /// its cleanup) can only ever affect that one pawn, and it is consumed by that
    /// pawn's next drop anyway. The marking patches clean up in Harmony finalizers,
    /// which run even when the original method throws.
    /// </summary>
    internal static class GlobalState
    {
        public static KeepYourGearSettings Settings => KeepYourGearMod.Settings;

        /// <summary>Debug logging is on only when its setting is enabled and dev mode is active.</summary>
        public static bool Debug => (Settings?.debugLogging ?? false) && Prefs.DevMode;

        /// <summary>Pawns whose weapon drop inside MakeDowned/Kill/Notify_PawnSpawned must be prevented.</summary>
        public static readonly HashSet<Pawn> KeepWeapon = new HashSet<Pawn>();

        /// <summary>Pawns whose inventory drop inside MakeDowned/Kill must be prevented.</summary>
        public static readonly HashSet<Pawn> KeepInventory = new HashSet<Pawn>();

        /// <summary>
        /// Pawns whose next single TryDropEquipment call must be prevented - used to
        /// suppress the weapon drop on manipulation loss in CheckForStateChange.
        /// </summary>
        public static readonly HashSet<Pawn> KeepEquippedWeapon = new HashSet<Pawn>();

        /// <summary>
        /// Colony members are everything of the player faction: colonists, slaves and
        /// colony animals alike. Prisoners, visitors and everyone else stay "other pawns".
        /// </summary>
        public static bool IsColonyMember(Pawn pawn)
        {
            return pawn.Faction == Faction.OfPlayer;
        }

        /// <summary>
        /// Whether a downed (dead == false) or killed (dead == true) pawn keeps its weapon.
        /// The "dead" options work standalone, independent of the two "downed" options.
        /// </summary>
        public static bool ShouldKeepWeapon(Pawn pawn, bool dead)
        {
            KeepYourGearSettings s = Settings;
            if (s == null)
            {
                return false;
            }
            return IsColonyMember(pawn)
                ? (dead ? s.keepWeaponsAndInventoryOfDeadColonists : s.keepColonistsWeapons)
                : (dead ? s.keepWeaponsAndInventoryOfOtherDeadPawns : s.keepOtherPawnsWeapons);
        }

        /// <summary>Inventory counterpart of <see cref="ShouldKeepWeapon"/>; on death one option covers both.</summary>
        public static bool ShouldKeepInventory(Pawn pawn, bool dead)
        {
            KeepYourGearSettings s = Settings;
            if (s == null)
            {
                return false;
            }
            return IsColonyMember(pawn)
                ? (dead ? s.keepWeaponsAndInventoryOfDeadColonists : s.keepColonistsInventory)
                : (dead ? s.keepWeaponsAndInventoryOfOtherDeadPawns : s.keepOtherPawnsInventory);
        }

        /// <summary>Log-friendly pawn name; animals and generated pawns may have none.</summary>
        public static string PawnName(Pawn pawn)
        {
            return pawn.Name is null ? pawn.ThingID : pawn.Name.ToStringShort;
        }
    }
}
