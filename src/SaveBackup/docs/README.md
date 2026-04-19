**Save Backup** is a [The Long Dark] mod that automatically backs up all your saves to its subfolder
once per real day. Particularly when playing with mods, this offers an escape hatch in case
something goes wrong (e.g. your save file gets corrupted).

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [Security](#security)
* [See also](#see-also)

## Install
1. Install [MelonLoader].
2. Download this mod's DLL directly into your game's `Mods` subfolder.
3. Launch the game.

## Use
Just play normally! When you launch the game:
1. A zip of your saves will be added to `Mods/SaveBackup` (if you don't have one for today).  
   _After a MelonLoader or game update, it may add an extra backup just in case they break saves._
2. It'll keep the last 10 backups by default (configurable).

This all happens in the background, so it doesn't affect the game's startup time.

To restore a backup, just copy the files from the zip back into your saves folder at
`%localappdata%\Hinterland\TheLongDark`. (You can paste that exact path into Windows Explorer.)

## Configure
From the game's Options menu, click "Mod Settings" and then navigate to "Save Backup". Hover the
cursor over a field for details.

> ![](images/config.png)

## Compatibility
The mod is compatible with The Long Dark 2.50+ and MelonLoader 0.7.2+, both survival mode and
Wintermute.

## Security
This mod is fully open-source. All its source code is public in this repository, so anyone can
verify that it's not doing anything malicious.

Each release also has a [public attestation][GitHub attestations], an unfalsifiable record which
proves exactly how the release file was created. That lets anyone verify that it _only_ contains
this code, and hasn't been modified in any way.

## See also
* [Release notes](release-notes.md)
* ~~Nexus mod~~ (not released yet)

[GitHub attestations]: https://docs.github.com/en/actions/concepts/security/artifact-attestations
[MelonLoader]: https://tldmods.net/install.html
[The Long Dark]: https://www.thelongdark.com
