
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class bagUI : MonoBehaviour
{
    public static bagUI Instance { get; private set; }
    private GameObject uiGameObject;
    private GameObject content;
    private GameObject itemPrefab;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        uiGameObject = transform.Find("UI").gameObject;
        content = transform.Find("UI/ListBg/Scroll View/Viewport/Content").gameObject;
    }

    // 展示背包
    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    // 关闭背包 
    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    // 添加物品
    public void AddItem(/* ItemSO itemSO */)
    {
        GameObject itemGo = GameObject.Instantiate(itemPrefab);
        itemGo.transform.parent = content.transform;
        itemUI itemUI = itemGo.GetComponent<itemUI>();
        string type = "";
        /*
            switch (itemSO.itemType)
            {
                case ItemType.Weapon:
                    type = "武器";
                case ItemType.Consumable:
                    type = "可消耗品";
            }

            itemUI.InitItem(itemSO.icon, itemSO.name, type);
        */
    }
}
