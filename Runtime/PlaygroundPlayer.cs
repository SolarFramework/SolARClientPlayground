using Unity.Netcode;
using UnityEngine;

namespace Bcom.SharedPlayground
{

    public class PlaygroundPlayer : NetworkBehaviour
    {
        private ClientNetworkTransform objectTransform;

        private void Awake()
        {
            objectTransform = FindObjectOfType<ClientNetworkTransform>();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("Player connected");
            base.OnNetworkSpawn();
        }

        [ServerRpc]
        public void CreateObjectServerRpc(ServerRpcParams serverRpcParams = default)
        {
            FindObjectOfType<NetworkObjectSpawner>().SpawnInstance(serverRpcParams.Receive.SenderClientId);
            Debug.Log("Spawned object!");
        }


        /// <summary>
        /// If this method is invoked on the client instance of this player, it will invoke a `ServerRpc` on the server-side.
        /// If this method is invoked on the server instance of this player, it will teleport player to a random position.
        /// </summary>
        /// <remarks>
        /// Since a `NetworkTransform` component is attached to this player, and the authority on that component is set to "Server",
        /// this transform's position modification can only be performed on the server, where it will then be replicated down to all clients through `NetworkTransform`.
        /// </remarks>
        [ClientRpc]
        public void RandomTeleportObjectClientRpc()
        {
            var oldPosition = objectTransform.transform.position;
            objectTransform.transform.position = GetRandomPositionOnXYPlane();
            var newPosition = objectTransform.transform.position;
            print($"{nameof(RandomTeleportObjectClientRpc)}() -> {nameof(OwnerClientId)}: {OwnerClientId} --- {nameof(oldPosition)}: {oldPosition} --- {nameof(newPosition)}: {newPosition}");
        }
        private static Vector3 GetRandomPositionOnXYPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
        }

    }
}