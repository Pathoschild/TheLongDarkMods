This repository contains my MelonLoader mods for [The Long Dark] **survival mode** (not Wintermute).
See the individual mods for documentation and release notes.

## Mods
* **Auto-Fill Map on Explore** <small>([readme](src/AutoFillMapOnExplore#readme))</small>  
  _Fills in the map around you as you explore._
* **Fast Travel** <small>([readme](src/FastTravel#readme))</small>  
  _Lets you save up to 9 fast travel points (e.g. your home base), and fast travel to them anytime
  at the press of a button._
* **Save Backup** <small>([readme](src/SaveBackup#readme))</small>  
  _Automatically backs up all your saves once per real day into its subfolder._

## Compiling the mods
Installing stable releases from [Nexus Mods] is recommended for most users. If you really want to
compile the mod yourself, read on.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:

1. Edit `src/Directory.Build.props` and change the `<GamePath>` value if needed.
2. Rebuild the project in [Visual Studio].  
   <small>This will compile the code and package it into the mod directory.</small>

### Compiling a mod for release
To package a mod for release:

1. Switch to `Release` build configuration.
2. Recompile the mod per the previous section.
3. Upload the generated `_releases/<mod name>.dll` files.

[Nexus Mods]: https://www.nexusmods.com/games/thelongdark
[The Long Dark]: https://www.thelongdark.com
[Visual Studio]: https://www.visualstudio.com/vs/community/
