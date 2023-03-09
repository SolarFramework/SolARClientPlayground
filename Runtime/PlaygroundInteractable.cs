using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: server side only
public class PlaygroundInteractable : MonoBehaviour
{
    public static readonly HashSet<PlaygroundInteractable> interactables = new HashSet<PlaygroundInteractable>();
    void Awake()
    {  
       interactables.Add(this); 
    }

    void OnDestroy()
    {
        interactables.Remove(this);
    }

    public static GameObject FindNearestInteractable(Vector3 position)
    {
        GameObject nearestGO = null;
        float distance = 10000f;
        foreach( var interactable in interactables)
        {
            float newDistance = (interactable.transform.position - position).magnitude;
            if( newDistance < distance)
            {
                distance = newDistance;
                nearestGO = interactable.gameObject;
            }
        }
        return nearestGO;
    }
}
