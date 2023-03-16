using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;


namespace Bcom.SharedPlayground
{
    [System.Serializable]
    public class Persistent3DObjects
    {
        [SerializeField]
        public List<PlaygroundInteractable.SerializableObjectData> persistentObjects = new List<PlaygroundInteractable.SerializableObjectData>();
    }

    // Note: Server only
    public class ScenePersistency : NetworkBehaviour
    {
        private static string PERSISTENT_SCENE_DATA_PATH => Path.Combine(Application.persistentDataPath, "sceneData.json");

        public PlaygroundPrefabsScriptableObject spawnablePrefabsList;

        public Transform sceneRoot;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                LoadSceneState();

            base.OnNetworkSpawn();
        }

        public void LoadSceneState()
        {
            if (!IsServer) return;

            if (!File.Exists(PERSISTENT_SCENE_DATA_PATH)) return;

            CleanupScene();

            var serializedData = File.ReadAllText(PERSISTENT_SCENE_DATA_PATH);
            var objectsToLoad = JsonUtility.FromJson<Persistent3DObjects>(serializedData);

            foreach (var objectToLoad in objectsToLoad.persistentObjects)
            {
                GameObject loadedObject = Instantiate(spawnablePrefabsList.prefabs[(int)objectToLoad.objectType], sceneRoot);
                loadedObject.name = objectToLoad.objectName;
                loadedObject.transform.localPosition = objectToLoad.position;
                loadedObject.transform.localRotation = objectToLoad.rotation;
                loadedObject.transform.localScale = objectToLoad.scale;
                // Call to Spawn the object on the network
                loadedObject.GetComponent<NetworkObject>().Spawn();
            }
            Debug.Log($"Loading scene from {PERSISTENT_SCENE_DATA_PATH}");
        }

        public void SaveSceneState()
        {
            if (!IsServer) return;

            Persistent3DObjects objectsToSave = new Persistent3DObjects();
            foreach (var interactable in PlaygroundInteractable.interactables)
            {
                objectsToSave.persistentObjects.Add(interactable.Serialize());
            }

            string jsonData = JsonUtility.ToJson(objectsToSave, true);
            File.WriteAllText(PERSISTENT_SCENE_DATA_PATH, jsonData);
            Debug.Log($"Saved {objectsToSave.persistentObjects.Count} persistent scene objects to {PERSISTENT_SCENE_DATA_PATH}");
        }

        public void CleanupScene()
        {
            if (!IsServer) return;

            foreach (var interactable in PlaygroundInteractable.interactables)
            {
                interactable.GetComponent<NetworkObject>().Despawn();
                Destroy(interactable);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SaveSceneStateServerRpc()
        {
            NetworkLog.LogInfoServer("Saving scene");
            SaveSceneState();
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadSceneStateServerRpc()
        {
            NetworkLog.LogInfoServer("Loading scene");
            LoadSceneState();
        }

        [ServerRpc(RequireOwnership = false)]
        public void CleanUpSceneServerRpc()
        {
            NetworkLog.LogInfoServer("Clean up scene");
            CleanupScene();
        }
    }

}