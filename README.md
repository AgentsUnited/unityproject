## Unity Projects
* **COUCHUnityProject & AgentsUnitedDemo**: The main projects for displaying agents from ASAP and GRETA in one scene. These also contains a basic interface overlay for controlling moves for each of the agents and the user.
* **CharacterCreatorNew**: Can be used to create and generate new [UMA](http://umadocs.secretanorak.com/doku.php?id=start) characters, which can then be imported as ASAP agents in other projects.
* **OculusQuest**: A very basic proof-of-concept project that is configured to run natively on the Oculus Quest VR headset using only ASAP agents.
* **WizardInterface**: A Wizard of Oz interface that is configured to run on an Android tablet or phone.  

## Build
The *AgentsUnitedDemo* project uses only free/open source assets and is configured out of the box with ASAP and GRETA agents.

The projects *COUCHUnityProject* and *OculusQuest* use some commercial 3rd party assets that cannot be made available in this repository. Please purchase and/or download these assets from the Unity Asset store and place them in  `{project}\Assets\Borg\3rdParty`:
* [Devotid Folding Table and Chair](https://assetstore.unity.com/packages/3d/props/furniture/folding-table-and-chair-pbr-111726)
* [DigitalKonstrukt Prototyping Pack](https://assetstore.unity.com/packages/3d/prototyping-pack-free-94277)
* [o3n Male and Female UMA Races](https://assetstore.unity.com/packages/3d/characters/humanoids/o3n-male-and-female-uma-races-102187)
* [o3n Modern Clothing Pack for Male](https://assetstore.unity.com/packages/3d/characters/modern-clothing-pack-for-o3n-male-120544)
* [o3n Modern Clothing Pack for Female](https://assetstore.unity.com/packages/3d/characters/modern-clothing-pack-for-o3n-female-116669)
* [RecompileDisabler](https://github.com/appetizermonster/Unity3D-RecompileDisabler/tree/master/Assets/RecompileDisabler)
* [RootMotion Final IK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)

## Run
1. Start Unity and open AgentsUnitedDemo, then open scene Assets/AgentsUnited/Scenes/MainScene.
1. You should see a room with 2 basic stools and a table. There should be 2 male grey agents on top of the stools (placeholders for ASAP agents) and 2 standing female agents (GRETA agents).
1. Follow the steps in the [demonstrator readme](https://github.com/AgentsUnited/demonstrator) to initialise and run the other modules that actually control the behaviour of the agents.

## Documentation
The following GameObjects/Components do most of the work in the scene.

### ASAPToolkit
The ASAPToolkit/ASAPToolkitManager connects to an external ASAPRealizer module, which controls the various ASAP agents movements and speech (see AudioStreaming/AudioStreamingReceiver). At initialisation time, the ASAPRealizer module requests the skeleton joint/bone map from the agents in the Unity scene. While running, it then streams joint rotations for the full skeleton to the Unity scene to animate each of the agents. Network connection parameters are configured in the various xxxMiddleware component scripts.

WorldObject components can be added to any GameObject to automatically make them available in BML as gaze/point targets.

### UMA_DCS
The [UMA](http://umadocs.secretanorak.com/doku.php?id=start) agents are used by ASAP. UMA uses recipes to define how an agent looks and what it is wearing (see UMA documentation for more details). When adding new recipes (or other library assets) make sure to update the global UMA library: in toolbar: UMA->Global Library Window->add individual assets or rebuild index

The UMA agents in the scene are COUCH_M_1 and COUCH_M_2, which are DynamicCharacterAvatars. The BasicCharacter component initialises each agent and adds it to the ASAPToolkitManager through its unique AgentID. The AgentID is used to send BML to specific agents. Our DynamicUMASkeleton component is responsible for mapping ASAPRealizer's joint representation onto an UMA agent's bones. 

### Canvas
The UIMiddlewareMoves displays a basic user interface overlay for controlling agent and user moves. Is generated dynamically at runtime based on the characters currently active in the ongoing dialogue. Toggle between user-controlled moves and automatic move selection. Buttons will typically display the text that will be spoken by the agent/user. May also be used for WoZ purposes.

### Camille and Laura
These agents connect to a running GRETA FML realizer module, which controls their animations and speech.

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
