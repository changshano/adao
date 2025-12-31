using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public static UImanager Instance;
    public Image hpMaskImage;
    public Image mpMaskImage;
    public Image exeMaskImage;
    private float originalSize_HPMP;  // 血条原始宽度
    private float originalSize_EXE;  // 经验条宽度

    void Awake()
    {
        Instance = this;
        originalSize_HPMP = hpMaskImage.rectTransform.rect.width;
        originalSize_EXE = exeMaskImage.rectTransform.rect.width;
    }

    /// <summary>
    /// 血条UI填充显示
    /// </summary>
    /// <param name="fillPercent">填充百分比</param>
    public void SetHPValue(float fillPercent)
    {
        hpMaskImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillPercent*originalSize_HPMP);
    }

    /// <summary>
    /// 蓝条UI填充显示
    /// </summary>
    /// <param name="fillPercent">填充百分比</param>
    public void SetMPValue(float fillPercent)
    {
        mpMaskImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillPercent * originalSize_HPMP);
    }

    /// <summary>
    /// 经验条UI填充显示
    /// </summary>
    /// <param name="fillPercent">填充百分比</param>
    public void SetEXEValue(float fillPercent)
    {
        exeMaskImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillPercent * originalSize_EXE);
    }
}
