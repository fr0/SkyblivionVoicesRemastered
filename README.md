# Skyblivion Voices Remastered

A Windows desktop app that takes the **new** voice recordings from The Elder Scrolls IV: Oblivion Remastered and turns them into a patch mod for Skyblivion.

Nothing from the Remaster is redistributed (I am assuming this is not allowed by Bethesda and/or Nexusmods).
The tool reads the data in your Oblivion Remastered installation, then converts the audio with the Creation Kit tools that ship with Skyrim Special Edition, and writes a mod folder you can add to your Skyblivion load order.

Once you have run the tool and created the patch, you can uninstall Oblivion Remastered (and also this tool).

## Requirements

* Oblivion Remastered installed (Steam/Game Pass folder containing `OblivionRemastered\Content\Dev\ObvData\Data`).
   * I don't have Game Pass, so this variant is completely untested. 
* Skyrim Special Edition with the Creation Kit installed. This tool uses two of its executables:
  `Tools\Audio\xWMAEncode.exe` (audio) and `Tools\LipGen\LipGenerator\LipGenerator.exe` (lip sync).
  Both are located automatically through Steam.
* Skyblivion installed into a Skyrim SE Data folder
* [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime) (not needed for the standalone version)

The .NET 10 SDK must be installed to compile the tool (most users won't need to do this).

## Installation

Extract the app somewhere on your hard drive and run `SkyblivionVoicesRemastered.App.exe`.
It does not need to be in the same folder as Skyblivion or Oblivion Remastered.

## Usage

1. Make sure the paths are auto-detected properly. If they are not, fill them in yourself.
   (e.g. if you're not a Steam user, you'll probably have to do this)
2. Click "Run all".
3. Wait.

**This will take a long time.** It takes about an hour and a half on my machine.

Settings are remembered between runs (`%LocalAppData%\SkyblivionVoicesRemastered\settings.json`), and the scan/map results live in the work folder.

If the app crashes for some reason, and you run it again, it **should** only run the parts that didn't finish.

## Status

As of the time of this writing, Skyblivion has not been released yet, so the mapping has only been exercised against a synthetic stand-in
(`synth` command: rebuilds the Remaster's quests, topics, responses and NPCs as a Skyrim plugin with fresh form IDs and per-quest topic splits).

I will expect to review `map`'s statistics and warnings against the real `Skyblivion.esm` before running a full `build`.

Known gaps:

* Lines from plugins Skyblivion does not include (Knights of the Nine, DLCs, AltarESPMain's own new dialogue) stay unmatched.
   * Futurework for when Skyblivion releases support for the DLCs 
* Lip sync for the new recordings is synthesized by the Creation Kit's FaceFX generator from the audio and the
  response text; the Remaster does not have these.
* Some Remaster archive entries are stray files (`*_alt`, `*_1v`, `dremoraf93`) with no dialogue record; they are listed but skipped.
