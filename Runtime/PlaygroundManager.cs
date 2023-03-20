using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

using UnityEditor;

namespace Bcom.SharedPlayground
{
    /// <summary>
    /// Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
    /// Once a connection has been established to the server, the local player can be teleported to random positions via a GUI button.
    /// </summary>
    public class PlaygroundManager : MonoBehaviour
    {
        public GameObject sceneRootPrefab;
        private ScenePersistency scenePersistency;

        public Vector2 scrollPosition;

        private void Start()
        {
#if UNITY_SERVER
            Debug.Log("Starting server");
            NetworkManager.Singleton.StartServer();
            Debug.Log($"Server started!");
            // Spawn scene root to manage playground objects
            var sceneRoot = Instantiate(sceneRootPrefab);
            scenePersistency = sceneRoot.GetComponent<ScenePersistency>();
            sceneRoot.GetComponent<NetworkObject>().Spawn();
#else
            // var unityTransport = GetComponent<UnityTransport>();
            // unityTransport.ConnectionData.Address = "172.18.248.191";
            // NetworkManager.Singleton.StartClient();
            // Debug.Log($"Connecting to server (ip={unityTransport.ConnectionData.Address})");
#endif // UNITY_SERVER
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsClient && !networkManager.IsServer)
            {
                if (GUILayout.Button("Host"))
                {
                    networkManager.StartHost();
                }

                if (GUILayout.Button("Client"))
                {
                    networkManager.StartClient();
                }

                if (GUILayout.Button("Server"))
                {
                    networkManager.StartServer();
                }
            }
            else
            {
                GUILayout.Label($"Mode: {(networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client")}");

                // "Random Teleport" button will only be shown to clients
                if (networkManager.IsClient)
                {

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    foreach (PrefabType prefabType in System.Enum.GetValues(typeof(PrefabType)))
                    {
                        if (GUILayout.Button($"Create {prefabType.ToString()}"))
                        {
                            if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer ))
                            {
                                playgroundPlayer.CreateObject(prefabType);
                            }
                        }
                    }

                    GUILayout.EndScrollView();


                    if (GUILayout.Button("Drop Object"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer ))
                        {
                            playgroundPlayer.DropObject();
                        }
                    }

                    if (GUILayout.Button("Grab Nearest Object"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer ))
                        {
                            var objectToGrab = PlaygroundInteractable.FindNearestInteractable(playgroundPlayer.transform.position);
                            if (objectToGrab)
                            {
                                playgroundPlayer.GrabObject(objectToGrab);
                            }
                        }
                    }

                    if (GUILayout.Button("Destroy Object"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.DestroyObject();
                        }
                    }

                    if (GUILayout.Button("LoadScene"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.DropObject();
                        }
                        FindObjectOfType<ScenePersistency>().LoadSceneStateServerRpc();
                    }
                    if (GUILayout.Button("SaveScene"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.DropObject();
                        }
                        FindObjectOfType<ScenePersistency>().SaveSceneStateServerRpc();
                    }
                    if (GUILayout.Button("ClearScene"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.DropObject();
                        }
                        FindObjectOfType<ScenePersistency>().CleanUpSceneServerRpc();
                    }
                }
                else
                {
                    if (GUILayout.Button("LoadScene"))
                    {
                        scenePersistency.LoadSceneState();
                    }
                    if (GUILayout.Button("SaveScene"))
                    {
                        scenePersistency.SaveSceneState();
                    }
                    if (GUILayout.Button("ClearScene"))
                    {
                        scenePersistency.CleanupScene();
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
