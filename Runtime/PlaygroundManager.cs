using Unity.Netcode;
using UnityEngine;

namespace Bcom.SharedPlayground
{
    /// <summary>
    /// Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
    /// Once a connection has been established to the server, the local player can be teleported to random positions via a GUI button.
    /// </summary>
    public class PlaygroundManager : MonoBehaviour
    {
        public GameObject[] prefabs;

#if UNITY_SERVER
        private void Start()
        {
            var networkManager = NetworkManager.Singleton;
            Debug.Log("Starting server");
            networkManager.StartServer();
            Debug.Log("Server started. hostname=" + networkManager.ConnectedHostname);
        }
#endif

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
                    if (GUILayout.Button("Random Teleport client object"))
                    {
                        if (networkManager.LocalClient != null)
                        {
                            if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                            {
                                playgroundPlayer.RandomTeleportObjectClientRpc();
                            }
                        }
                    }
                    if (GUILayout.Button("Create cube"))
                    {
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
                        {
                            playgroundPlayer.CreateObjectServerRpc();
                        }
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
