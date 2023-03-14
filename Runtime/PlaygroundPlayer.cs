using Unity.Netcode;
using UnityEngine;

namespace Bcom.SharedPlayground
{

    public class PlaygroundPlayer : NetworkBehaviour
    {
        public PlaygroundPrefabsScriptableObject spawnablePrefabsList;

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

        public void CreateObject(PrefabType prefabType)
        {
            // Check if the player was already owning an object
            if (grabbedObject != null)
            {
                Debug.LogWarning("Player has already grabbed an object!");
                return;
            }
            // Create the new object and give its ownership to the player
            CreateObjectServerRpc(prefabType);
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

        [ClientRpc]
        public void GrabObjectClientRpc(NetworkObjectReference clientObjectRef)
        {
            NetworkLog.LogInfoServer("Grabbed item");
            grabbedObject = clientObjectRef;
        }

        [ClientRpc]
        public void DropObjectClientRpc()
        {
            NetworkLog.LogInfoServer("Dropped item");
            grabbedObject = null;
        }

        [ServerRpc]
        public void CreateObjectServerRpc(PrefabType prefabType, ServerRpcParams serverRpcParams = default)
        {
            grabbedObject = Instantiate(spawnablePrefabsList.prefabs[(int)prefabType]);
            var newNetworkObject = grabbedObject.GetComponent<NetworkObject>();
            newNetworkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
            newNetworkObject.DontDestroyWithOwner = true;
            NetworkLog.LogInfoServer("Spawned object!");

            // Notify all clients that this player has grabbed a new object
            GrabObjectClientRpc(newNetworkObject);
        }

        [ServerRpc]
        public void RemoveObjectOwnershipServerRpc(NetworkObjectReference clientObjectRef, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject networkObject = clientObjectRef;
            networkObject.RemoveOwnership();
            NetworkLog.LogInfoServer("Player gave object ownership back to the server");

            // Notify all clients that this player has dropped its object
            DropObjectClientRpc();
        }

        [ServerRpc]
        public void RequestObjectOwnershipServerRpc(NetworkObjectReference serverObjectRef, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject networkObject = serverObjectRef;
            NetworkLog.LogInfoServer("Player requested object ownership");
            // Check if the object is not already grabbed by another player
            if (!networkObject.IsOwnedByServer)
            {
                NetworkLog.LogInfoServer("WARNING: Object is still in use by another player!");
                return;
            }

            networkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

            // Notify all clients that this player has grabbed a new object
            GrabObjectClientRpc(networkObject);
        }
    }
}