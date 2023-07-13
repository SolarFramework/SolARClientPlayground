using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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

        private Vector2 scrollPosition;

        public ushort port;
        public string overrideIP = "";

        private bool connected = false;

        private void Start()
        {
#if UNITY_SERVER
            var unityTransport = GetComponent<UnityTransport>();
            Debug.Log("Starting server");
            NetworkManager.Singleton.StartServer();
            Debug.Log($"Server started! Listening on {unityTransport.ConnectionData.Address}:{unityTransport.ConnectionData.Port}");
            // Spawn scene root to manage playground objects
            var sceneRoot = Instantiate(sceneRootPrefab);
            scenePersistency = sceneRoot.GetComponent<ScenePersistency>();
            sceneRoot.GetComponent<NetworkObject>().Spawn();
#endif // UNITY_SERVER
        }

        protected void Configure()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        }

        protected void Connect(string frontendIp)
        {
            var unityTransport = GetComponent<UnityTransport>();
            // Use same ip as SolAR frontend (without http:// and port)
            string ipWithoutHTTP = overrideIP == "" ? frontendIp.Substring(7) : overrideIP;
            unityTransport.ConnectionData.Address = ipWithoutHTTP.Split(':')[0];
            unityTransport.ConnectionData.Port = port;
            Debug.Log($"Connecting to 3D Assets Sync server at {unityTransport.ConnectionData.Address}:{unityTransport.ConnectionData.Port}");

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client connection");
            }
        }

        protected void Disconnect()
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
            {
                playgroundPlayer.Disconnect();
                connected = false;
                Debug.Log("Client disconnected");
            }
        }

        private void OnConnect(ulong clientId)
        {
            Debug.Log("Client connection successful!");
            connected = true;
        }

        private void OnDisconnect(ulong clientId)
        {
            if (connected)
            {
                Disconnect();
            }
            else
            {
                Debug.LogWarning("OnDisconnect: Client not connected!");
            }
        }

#if UNITY_EDITOR
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
                    if (GUILayout.Button("Disconnect"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.Disconnect();
                        }
                    }

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    for (int i = 0; i < NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count; ++i)
                    {
                        var prefab = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[i];
                        if (GUILayout.Button($"Create {prefab.Prefab.name}"))
                        {
                            if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                            {
                                playgroundPlayer.CreateObject(i, FindObjectOfType<ScenePersistency>().gameObject);
                            }
                        }
                    }

                    GUILayout.EndScrollView();


                    if (GUILayout.Button("Drop Object"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.DropObject();
                        }
                    }

                    if (GUILayout.Button("Grab Nearest Object"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
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
#endif // UNITY_EDITOR
    }
}
