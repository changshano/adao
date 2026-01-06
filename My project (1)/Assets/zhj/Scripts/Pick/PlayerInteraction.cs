using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键点击
        {
            TryInteractWithObject();
        }
    }
    
    void TryInteractWithObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        Debug.Log("发射射线检测点击...");
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log($"射线命中：{hit.collider.gameObject.name}");
            
            // 方法1：通过标签检测
            if (hit.collider.CompareTag("Interactable"))
            {
                Debug.Log("检测到可交互物体（通过标签）");
                InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.OnClick(transform);
                }
            }
            
            // 方法2：直接检测组件（更可靠）
            InteractableObject interactableDirect = hit.collider.GetComponent<InteractableObject>();
            if (interactableDirect != null)
            {
                Debug.Log("检测到可交互物体（通过组件）");
                interactableDirect.OnClick(transform);
            }
        }
        else
        {
            Debug.Log("射线未命中任何物体");
        }
    }
}