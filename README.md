# Stationeers mod "Mirrored Devices" for BepInEx.

previously released as "Mirrored Atmospherics"

# Description:  
Adds a mirrored variant for each atmospherics and phase change devices:
- Air Conditioner
- Filtration
- Electrolyser
- Nitrolyser
- H2 Combustor
- Evaporation Chamber
- Condensation Chamber

# Instructions

## Easy install (StationeersLaunchPad)
Install BepInEx with the StationeersLaunchPad plugin.
See: https://github.com/StationeersLaunchPad/StationeersLaunchPad

Add the mod from Steam: https://steamcommunity.com/workshop/filedetails/?id=3614893812

## Manual install

* If you don't have BepInEx installed, download the lastest 5.x  64 bit version available at https://github.com/BepInEx/BepInEx/releases and follow the BepInEx installation instructions but basically you will need to:
     - Drop it inside Stationeers folder
     - Start the game once to finish installing BepInEx and check if he created the folders called \Stationeers\BepInEx\plugins, if yes, the BepInEx installation is completed.
* Download the lastest release from https://github.com/VincentCharpentier/Stationeers-Mirrored-Athmospherics/releases/ page.
* Unpack it inside the folder \BepInEx\plugins
* Start the game.

# Contributions

If you want to contribute with this mod, feel free to create a pull request.

To build from source you will need to add a `Directory.Build.targets` file at the root of the repository with the following content, and customize the `GameDir` path to point to your Stationeers installation:
```xml
<Project>
	<PropertyGroup>
		<GameDir>E:\SteamLibrary\steamapps\common\Stationeers</GameDir>
		<ReferencePath>
			$(GameDir)\Stationeers_Data\Managed\
		</ReferencePath>
		<AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
	</PropertyGroup>
</Project>
```
