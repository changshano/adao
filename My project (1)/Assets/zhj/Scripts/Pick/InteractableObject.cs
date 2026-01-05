using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public void OnClick(UnityEngine.AI.NavMeshAgent playerAgent)
    {
        playerAgent.SetDestination(transform.position);
        Interact();

    }
    protected virtual void Interact()
    {
        print("Interacting with Interactable Object.");
    }
}
