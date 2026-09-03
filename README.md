# Reequip Weapon Upon Recovery
![alt text](About/Preview.png)

Keep weapon and/or inventory items when a pawn is downed instead of throwing everything on the ground.  
Successor of "Do Not Drop Weapon" and "Where Is My Weapon?" with better implementation.  

## How it works

The mod marks a pawn at the exact moments vanilla would strip it - being downed
(`Pawn_HealthTracker.MakeDowned`), dying (`Pawn.Kill`), spawning downed
(`Pawn_EquipmentTracker.Notify_PawnSpawned`) and losing the use of its hands
(`Pawn_HealthTracker.CheckForStateChange`) - and prevents only those gear drops,
per settings. Everything else keeps vanilla behavior:

- **Stripping always works** - on downed pawns, prisoners and corpses.
- **Capture, trading and quest corpses stay vanilla** - gear drops when a pawn is
  captured, sold or spawned as a quest corpse, exactly like without the mod.
- **Kidnapping leaves the gear behind** - vanilla strips a pawn when it goes down, so
  a pawn carried off by hostiles never had gear on it. With this mod it would, so a
  pawn picked up by someone hostile to it drops its gear at the pickup spot instead of
  leaving the map with the kidnapper. Rescue by its own faction carries the gear along.
- **Destroyed hands no longer disarm** - vanilla drops the weapon of a standing pawn
  that loses its manipulation capacity; the mod prevents that drop too (the weapon
  stays equipped and is usable again once the pawn can hold things).
- **No global state** - all decisions are pawn-keyed and cleaned up in Harmony
  finalizers, so an exception mid-drop can never leak onto another pawn.

## Things to keep in mind

Deliberate trade-offs the mod makes to stay balanced:

- A downed enemy who recovers wakes up still armed - keeping gear means keeping it for
  everyone, including your foes.
- Kept gear on dead pawns stays on the corpse: strip the body to take it, or it is
  destroyed when the corpse rots away.

## Settings
Check mod settings.  
There you can find separate options for the player's colonists, enemies, inventory and
weapons. The two "dead" options work standalone - a colonist can drop everything when
downed but keep it on death, or the other way around. Colony animals and slaves count
as colonists; prisoners and visitors count as other pawns.

## Why such a misleading name?
I started this project with the intention of simply reimplementing existing mods and named it accordingly, but it ended up being implemented much differently.  

## Steam Workshop
https://steamcommunity.com/sharedfiles/filedetails/?id=3234461612.

## Build from source

Requires the .NET SDK.

```
cd Source/KeepYourGear
dotnet build -c Release -p:RimWorldDir="C:\Path\To\RimWorld"
```

Add `-p:HarmonyDir="C:\Path\To\Harmony"` if Harmony is not installed at the default
Steam Workshop location
(`...\steamapps\workshop\content\294100\2009463077\Current\Assemblies`).

The output lands in `Assemblies/KeepYourGear.dll`; the whole `KeepYourGear` folder can
be junctioned or copied into the game's `Mods` directory.
