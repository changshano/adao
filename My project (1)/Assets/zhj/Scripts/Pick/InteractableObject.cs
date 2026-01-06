using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float interactDistance = 3f;
    
    public void OnClick(Transform playerTransform)
    {
        float distance = Vector3.Distance(playerTransform.position, transform.position);
        Debug.Log($"点击物体：{gameObject.name}，距离：{distance}");
        
        if (distance <= interactDistance)
        {
            Interact();
        }
        else
        {
            Debug.Log("距离太远，无法交互，当前距离：" + distance);
        }
    }
    
    protected virtual void Interact()
    {
        Debug.Log("基础交互完成");
    }
}
