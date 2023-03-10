using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabList", menuName = "SharedPlayground/PlaygroundPrefabs")]
public class PlaygroundPrefabsScriptableObject : ScriptableObject
{
    public List<GameObject> prefabs;
}
