using System.Collections.Generic;
using Unity.Netcode;
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
#if UNITY_SERVER
        private void Start()
        {
            var networkManager = NetworkManager.Singleton;
            Debug.Log("Starting server");
            networkManager.StartServer();
            Debug.Log("Server started!");
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
                            playgroundPlayer.GrabObject(objectToGrab);
                        }
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
