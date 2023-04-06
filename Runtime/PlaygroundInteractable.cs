using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bcom.SharedPlayground
{

    public enum PrefabType
    {
        Cube,
        Sphere,
        Arrow,
        Stand_4dviews,
        Stand_actronika,
        Stand_artefacto,
        Stand_bcom,
        Stand_immersivedisplay,
        Stand_inod,
        Stand_inovr,
        Stand_inria,
        Stand_lynx,
        Stand_meta,
        Stand_noisemakers,
        Stand_pico,
        Stand_senseglove,
        Exit,
        Mascotte,
        // TODO: add more object prefabs
    };

    public class PlaygroundInteractable : MonoBehaviour
    {
        public static readonly HashSet<PlaygroundInteractable> interactables = new HashSet<PlaygroundInteractable>();
        AnimateObject animateObject;

        void Awake()
        {
            interactables.Add(this);
            TryGetComponent<AnimateObject>(out animateObject);
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
            public PrefabType objectType = PrefabType.Cube;
        };

        public PrefabType m_type = PrefabType.Cube;

        public SerializableObjectData Serialize()
        {
            SerializableObjectData objectData = new SerializableObjectData();
            objectData.objectName = name;
            objectData.position = transform.localPosition;
            objectData.rotation = transform.localRotation;
            objectData.scale = transform.localScale;
            objectData.objectType = m_type;

            return objectData;
        }
    }
}