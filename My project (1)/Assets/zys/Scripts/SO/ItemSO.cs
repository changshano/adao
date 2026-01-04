using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ItemSO : ScriptableObject
{
    public string name;
    public ItemType itemType;
    public string description;
    public List<ItemProperty> propertyList;
    public Sprite icon;
    public GameObject prefab;
}
 
public enum ItemType
{
    Weapen,
    Consumable
}

[Serializable]
public class ItemProperty
{
    public ItemPropertyType propertyType;
    public int value;
}

public enum ItemPropertyType
{
    HPValue,
    DefenseValue,
    AttackValue
}