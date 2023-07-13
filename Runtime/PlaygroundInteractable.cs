using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Bcom.SharedPlayground
{
    public class PlaygroundInteractable : MonoBehaviour
    {
        public static readonly HashSet<PlaygroundInteractable> interactables = new HashSet<PlaygroundInteractable>();

        public NetworkPrefabsList networkPrefabsList;
        AnimateObject animateObject;

        void Awake()
        {
            if (!networkPrefabsList)
            {
                Debug.LogError("PlaygroundInteractable is missing it's NetworkPrefabsList!");
            }

            interactables.Add(this);
            TryGetComponent(out animateObject);
        }

        void OnDestroy()
        {
            interactables.Remove(this);
        }

        public void Animate(bool value)
        {
            if (animateObject != null)
                animateObject.enabled = value;
        }

        public static GameObject FindNearestInteractable(Vector3 position)
        {
            GameObject nearestGO = null;
            float distance = 10000f;
            foreach (var interactable in interactables)
            {
                float newDistance = (interactable.transform.position - position).magnitude;
                if (newDistance < distance)
                {
                    distance = newDistance;
                    nearestGO = interactable.gameObject;
                }
            }
            return nearestGO;
        }

        [System.Serializable]
        public class SerializableObjectData
        {
            public string objectName = "";
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
            public int prefabIndex = 0;
        };

        public SerializableObjectData Serialize()
        {
            SerializableObjectData objectData = new SerializableObjectData();
            objectData.objectName = name;
            objectData.position = transform.localPosition;
            objectData.rotation = transform.localRotation;
            objectData.scale = transform.localScale;
            objectData.prefabIndex = GetNetworkPrefabIndex();

            return objectData;
        }

        private int GetNetworkPrefabIndex()
        {
            for (int i = 0; i < networkPrefabsList.PrefabList.Count; ++i)
            {
                if (name.Contains(networkPrefabsList.PrefabList[i].Prefab.name))
                    return i;
            }
            Debug.LogError("Failed to find matching NetworkPrefab for this object");
            return -1;
        }
    }
}