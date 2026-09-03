# Reequip Weapon Upon Recovery
![alt text](About/Preview.png)

Keep weapon and/or inventory items when a pawn is downed instead of throwing everything on the ground.  
Successor of "Do Not Drop Weapon" and "Where Is My Weapon?" with better implementation.  

## Settings
Check mod settings.  
There you can find separate options for the player's colonists, enemies, inventory and weapons.  

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
