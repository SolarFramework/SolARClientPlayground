# SharedPlayground - Unity Package for 3D Assets Synchronization

## How to use

To start the server from the editor, simply load the DemoScene in `Samples/Demo`.

## How to build the server application

Create an empty Unity project and import the SharedPlayground package.

Then use the following command to build the server into a headless executable:

```powershell
"PATH_TO_UNITY_FOLDER\Unity.exe" -batchmode -nographics -logFile "PATH_TO_LOG_FILE" -projectPath "PATH_TO_THE_PROJECT" -executeMethod Bcom.SharedPlayground.BuildServer.PerformDedicatedServerBuild -buildPath "OUTPUT_DIR" -target ("Win64"|"Linux64") -quit
```

## How to add a new 3D asset

### On the server side

Register a new PrefabType enum value in the `PlaygroundInteractable.cs` script.

Create a prefab variant of `Demo/Prefabs/SP_Object.prefab`, rename it accordingly (ie. "SP_YourNewAssetName") change the Type value in the PlaygroundInteractable component to be your new asset type.

From this point you can customize the new prefab with a mesh, material and idle animation (cf. other existing asset prefabs for reference and examples).

Register the newly created prefab variant in the `PrefabList.asset` file, by adding a new element entry and linking your new prefab.

Then, modify the NetworkManager's component in the `SharedPlayground.prefab` file, to add the new asset prefab to the list of NetworkPrefabs.

### On the client side

TODO
