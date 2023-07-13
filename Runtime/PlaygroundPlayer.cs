using Unity.Netcode;
using UnityEngine;

namespace Bcom.SharedPlayground
{

    public class PlaygroundPlayer : NetworkBehaviour
    {
        public NetworkPrefabsList spawnablePrefabsList;

        public GameObject grabbedObject = null;

        public override void OnNetworkSpawn()
        {
            NetworkLog.LogInfoServer("Player connected");
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            NetworkLog.LogInfoServer("Player disconnected");
            base.OnNetworkDespawn();
        }

        public void CreateObject(int prefabIndex, GameObject sceneRoot)
        {
            // Check if the player was already owning an object
            if (grabbedObject != null)
            {
                Debug.LogWarning("Player has already grabbed an object!");
                return;
            }
            // Create the new object and give its ownership to the player
            CreateObjectServerRpc(prefabIndex, sceneRoot);
        }

        public void DropObject()
        {
            if (grabbedObject == null) 
            {
                Debug.LogWarning("No grabbed object at the moment, nothing to drop!");
                return;
            }

            RemoveObjectOwnershipServerRpc(grabbedObject);
        }

        public void GrabObject(GameObject objectTograb)
        {
            if (grabbedObject != null)
            {
                Debug.LogWarning("Player has already grabbed an object!");
                return;
            }

            RequestObjectOwnershipServerRpc(objectTograb);
        }

        public void DestroyObject()
        {
            if (grabbedObject == null)
            {
                Debug.LogWarning("No grabbed object at the moment, nothing to destroy!");
                return;
            }

            DespawnObjectServerRpc(grabbedObject);
        }

        public void Disconnect()
        {
            DropObject();
            RequestDisconnectServerRpc();
        }

        [ClientRpc]
        public void GrabObjectClientRpc(NetworkObjectReference clientObjectRef)
        {
            NetworkLog.LogInfoServer("Grabbed item");
            grabbedObject = clientObjectRef;
        }

        [ClientRpc]
        public void DropObjectClientRpc(NetworkObjectReference serverObjectRef)
        {
            GameObject serverObject = serverObjectRef;
            NetworkLog.LogInfoServer("Dropped item");
            grabbedObject = null;
        }

        [ServerRpc]
        public void CreateObjectServerRpc(int prefabIndex, NetworkObjectReference sceneRootRef, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject sceneRoot = sceneRootRef;
            grabbedObject = Instantiate(spawnablePrefabsList.PrefabList[prefabIndex].Prefab);
            grabbedObject.GetComponent<PlaygroundInteractable>().Animate(false);
            var newNetworkObject = grabbedObject.GetComponent<NetworkObject>();
            newNetworkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
            grabbedObject.transform.parent = sceneRoot.transform;
            newNetworkObject.DontDestroyWithOwner = true;
            NetworkLog.LogInfoServer("Spawned object!");

            // Notify all clients that this player has grabbed a new object
            GrabObjectClientRpc(newNetworkObject);
        }

        [ServerRpc]
        public void RemoveObjectOwnershipServerRpc(NetworkObjectReference clientObjectRef, ServerRpcParams serverRpcParams = default)
        {
            grabbedObject = null;
            NetworkObject networkObject = clientObjectRef;
            networkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            NetworkLog.LogInfoServer("Player gave object ownership back to the server");
            networkObject.GetComponent<PlaygroundInteractable>().Animate(true);
            // Notify all clients that this player has dropped its object
            DropObjectClientRpc(networkObject);
        }

        [ServerRpc]
        public void RequestObjectOwnershipServerRpc(NetworkObjectReference serverObjectRef, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject networkObject = serverObjectRef;
            networkObject.GetComponent<PlaygroundInteractable>().Animate(false);
            NetworkLog.LogInfoServer("Player requested object ownership");
            // Check if the object is not already grabbed by another player
            if (!networkObject.IsOwnedByServer)
            {
                NetworkLog.LogInfoServer("WARNING: Object is still in use by another player!");
                return;
            }

            networkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);
            grabbedObject = networkObject.gameObject;
            // Notify all clients that this player has grabbed a new object
            GrabObjectClientRpc(networkObject);
        }

        [ServerRpc]
        public void DespawnObjectServerRpc(NetworkObjectReference clientObjectRef)
        {
            // Notify all clients that this player has dropped its object
            DropObjectClientRpc(clientObjectRef);

            NetworkObject networkObject = clientObjectRef;
            networkObject.Despawn();
        }

        [ServerRpc]
        public void RequestDisconnectServerRpc(ServerRpcParams serverRpcParams = default)
        {
            NetworkLog.LogInfoServer("Player sent disconnect");
            NetworkManager.Singleton.DisconnectClient(serverRpcParams.Receive.SenderClientId);
        }
    }
}