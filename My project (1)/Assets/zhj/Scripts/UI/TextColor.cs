using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RainbowGradientText : MonoBehaviour
{
    private TMP_Text textComponent;
    public float speed = 1f;
    public Gradient rainbowGradient;
    
    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.enableVertexGradient = true;
        
        // 创建彩虹渐变
        rainbowGradient = new Gradient();
        
        // 设置彩虹色
        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.red, 0.0f),
            new GradientColorKey(Color.yellow, 0.16f),
            new GradientColorKey(Color.green, 0.33f),
            new GradientColorKey(Color.cyan, 0.5f),
            new GradientColorKey(Color.blue, 0.66f),
            new GradientColorKey(Color.magenta, 0.83f),
            new GradientColorKey(Color.red, 1.0f)
        };
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        
        rainbowGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    void Update()
    {
        ApplyRainbowGradient();
    }
    
    void ApplyRainbowGradient()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        
        if (textInfo.characterCount == 0) return;
        
        // 获取文本边界
        Bounds bounds = textComponent.bounds;
        float minX = bounds.min.x;
        float width = bounds.size.x;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;
            
            int vertexIndex = charInfo.vertexIndex;
            
            // 计算每个字符在文本中的位置比例
            float charX = textInfo.meshInfo[0].vertices[vertexIndex].x;
            float normalizedPosition = (charX - minX) / width;
            
            // 添加时间因素使渐变流动
            float timeOffset = Time.time * speed;
            float gradientPos = (normalizedPosition + timeOffset) % 1.0f;
            
            Color charColor = rainbowGradient.Evaluate(gradientPos);
            
            // 为字符的四个顶点设置颜色
            for (int j = 0; j < 4; j++)
            {
                textInfo.meshInfo[0].colors32[vertexIndex + j] = charColor;
            }
        }
        
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}