using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class itemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;

    // 初始化item（更新item）
    public void InitItem(Sprite iconSprite, string name, string type)
    {
        iconImage.sprite = iconSprite;
        nameText.text = name;
        typeText.text = type;
    }
}
