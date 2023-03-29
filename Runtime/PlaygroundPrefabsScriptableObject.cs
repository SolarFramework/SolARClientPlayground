using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bcom.SharedPlayground
{
    [CreateAssetMenu(fileName = "PrefabList", menuName = "SharedPlayground/PlaygroundPrefabs")]
    public class PlaygroundPrefabsScriptableObject : ScriptableObject
    {
        public List<GameObject> prefabs;
    }
}
