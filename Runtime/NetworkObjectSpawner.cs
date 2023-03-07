

using UnityEngine;
using Unity.Netcode;

namespace Bcom.SharedPlayground
{
    // Spawns an object when connection is started, then destroys it when disconnecting
    public class NetworkObjectSpawner : NetworkBehaviour
    {
        public GameObject prefabToSpawn;
        public bool destroyWithSpawner;

        private GameObject m_prefabInstance;
        private NetworkObject m_spawnedNetworkObject;

        public override void OnNetworkSpawn()
        {
            // Only the server spawns, clients will disable this component on their side
            enabled = IsServer;
            if (prefabToSpawn == null)
            {
                Debug.LogWarning("No prefab set");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && destroyWithSpawner && m_spawnedNetworkObject != null && m_spawnedNetworkObject.IsSpawned)
            {
                m_spawnedNetworkObject.Despawn();
            }
            base.OnNetworkDespawn();
        }

        public void SpawnInstance(ulong clientId)
        {
            // Instantiate the GameObject Instance
            m_prefabInstance = Instantiate(prefabToSpawn);

            // Optional, this example applies the spawner's position and rotation to the new instance
            m_prefabInstance.transform.position = transform.position;
            m_prefabInstance.transform.rotation = transform.rotation;

            // Get the instance's NetworkObject and Spawn
            m_spawnedNetworkObject = m_prefabInstance.GetComponent<NetworkObject>();
            m_spawnedNetworkObject.SpawnWithOwnership(clientId);
        }
    }
}