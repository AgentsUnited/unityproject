## Unity Projects
* **COUCHUnityProject**:
* **CharacterCreatorNew**:
* **OculusQuest**:
* **WizardInterface**:

## Build
The projects *COUCHUnityProject* and *OculusQuest* use some commercial 3rd party assets that cannot be made available in this repository. Please purchase and/or download these assets from the Unity Asset store and place them in  `{project}\Assets\Borg\3rdParty`:
    - [Devotid Folding Table and Chair](https://assetstore.unity.com/packages/3d/props/furniture/folding-table-and-chair-pbr-111726)
    - [DigitalKonstrukt Prototyping Pack](https://assetstore.unity.com/packages/3d/prototyping-pack-free-94277)
    - [o3n Male and Female UMA Races](https://assetstore.unity.com/packages/3d/characters/humanoids/o3n-male-and-female-uma-races-102187)
    - [o3n Modern Clothing Pack for Male](https://assetstore.unity.com/packages/3d/characters/modern-clothing-pack-for-o3n-male-120544)
    - [o3n Modern Clothing Pack for Female](https://assetstore.unity.com/packages/3d/characters/modern-clothing-pack-for-o3n-female-116669)
    - [RecompileDisabler](https://github.com/appetizermonster/Unity3D-RecompileDisabler/tree/master/Assets/RecompileDisabler)
    - [RootMotion Final IK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)

## Run
1. Start Unity and open the Character Creator project.
2. Open a Command Prompt and navigate to HmiMultiAgentAsap\java
    1. `ant run`

## Troubleshooting

### Using GIT with UNITY
We use GIT with Unity, but there may be merge conflicts if you push and pull your project. The following workflow seems to be ok to solve the conflicts:
1. First, install DiffMerge. This is the graphical tool that you will use to merge conflicts in Unity asset and scene files.
2. Second, add `UnityYAMLMerge.exe` to your windows PATH variable. This tool will automatically try to merge different versions of the assets. It should be installed in your Unity folder, somewhere like: `C:\Program Files\Unity\Hub\Editor\2017.4.32f1\Editor\Data\Tools\`
3. Then, configure GIT to use UnityYAMLMerge. Add the following to your `~\.gitconfig` file: 
```
	[merge]
		tool = unityyamlmerge
	[mergetool "unityyamlmerge"]
		trustExitCode = false
		cmd = 'UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```
If there are conflicts after a commit, pull or stash pop or whatever, GIT will tell you about them. 
You can then enter command `git mergetool` which will call the UnityYAMLMerge automatically. If there are still conflicts that then can not be resolved automagically, it will launch the graphical editor DiffMerge. You make your merges here by hand by picking changes from your file and the remote file (left is local, middle is the resolved new file, right is remote file).

## License
These projects are licensed under the GNU Lesser General Public License v3.0 (LGPL 3.0).
