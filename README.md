# Using GIT with UNITY
We use GIT with Unity, but there may be merge conflicts if you push and pull your project.
The following workflow seems to be ok to solve the conflicts.

First, install DiffMerge. This is the graphical tool that you will use to merge conflicts in Unity asset and scene files.

Second, add UnityYAMLMerge.exe to your windows PATH variable. This tool will automatically try to merge different versions of the assets.
It should be installed in your Unity folder, somewhere like: C:\Program Files\Unity\Hub\Editor\2017.4.32f1\Editor\Data\Tools\
Then, configure GIT to use UnityYAMLMerge.  
Add the following to your ~\.gitconfig file: 

	[merge]
		tool = unityyamlmerge
	[mergetool "unityyamlmerge"]
		trustExitCode = false
		cmd = 'UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"

If there are conflicts after a commit, pull or stash pop or whatever, GIT will tell you about them. 
You can then enter command "git mergetool" which will call the UnityYAMLMerge automatically. If there are still conflicts that then can not be resolved automagically, it will launch the graphical editor DiffMerge. You make your merges here by hand by picking changes from your file and the remote file (left is local, middle is the resolved new file, right is remote file)

#Run
1. Start Unity and open the Character Creator project.
2. Open a Command Prompt and navigate to HmiMultiAgentAsap\java
	a. ant run