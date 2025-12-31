// ExperienceGrowthSystem/UI/ExperienceBar.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBar : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Image fillImage;
    
    [Header("颜色设置")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color nearLevelUpColor = Color.yellow;
    [SerializeField] private Color levelUpColor = Color.green;
    
    [Header("动画设置")]
    [SerializeField] private float fillSpeed = 5f;
    [SerializeField] private float levelUpEffectDuration = 1f;
    
    private float targetFillAmount = 0f;
    private bool isLevelingUp = false;
    
    private void Start()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.onExperienceChanged.AddListener(OnExperienceChanged);
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUp.AddListener(OnLevelUp);
        }
        
        // 初始更新
        UpdateUI();
    }
    
    private void Update()
    {
        if (experienceSlider != null)
        {
            // 平滑填充经验条
            experienceSlider.value = Mathf.Lerp(experienceSlider.value, targetFillAmount, Time.deltaTime * fillSpeed);
            
            // 更新颜色
            UpdateFillColor();
        }
    }
    
    private void OnExperienceChanged(int currentExp, int expToNextLevel)
    {
        if (expToNextLevel <= 0) return;
        
        float progress = (float)currentExp / expToNextLevel;
        targetFillAmount = progress;
        
        UpdateText(currentExp, expToNextLevel);
    }
    
    private void OnLevelUp(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{newLevel}";
        }
        
        // 触发升级效果
        StartCoroutine(LevelUpEffect());
    }
    
    private void UpdateText(int currentExp, int expToNextLevel)
    {
        if (expText != null)
        {
            expText.text = $"{currentExp} / {expToNextLevel}";
        }
    }
    
    private void UpdateFillColor()
    {
        if (fillImage == null) return;
        
        float progress = experienceSlider.value;
        
        if (isLevelingUp)
        {
            // 升级时的闪烁效果
            float pingPong = Mathf.PingPong(Time.time * 10f, 1f);
            fillImage.color = Color.Lerp(levelUpColor, Color.white, pingPong);
        }
        else if (progress > 0.8f)
        {
            // 接近升级时的黄色
            fillImage.color = Color.Lerp(normalColor, nearLevelUpColor, (progress - 0.8f) * 5f);
        }
        else
        {
            fillImage.color = normalColor;
        }
    }
    
    private System.Collections.IEnumerator LevelUpEffect()
    {
        isLevelingUp = true;
        
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (elapsedTime < levelUpEffectDuration)
        {
            float scaleMultiplier = 1f + Mathf.Sin(elapsedTime * Mathf.PI * 2f) * 0.1f;
            transform.localScale = originalScale * scaleMultiplier;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        isLevelingUp = false;
    }
    
    private void UpdateUI()
    {
        if (ExperienceManager.Instance != null && LevelManager.Instance != null)
        {
            int currentExp = ExperienceManager.Instance.GetCurrentExperience();
            int expToNext = ExperienceManager.Instance.GetExpToNextLevel();
            int currentLevel = LevelManager.Instance.GetCurrentLevel();
            
            if (levelText != null)
            {
                levelText.text = $"Lv.{currentLevel}";
            }
            
            OnExperienceChanged(currentExp, expToNext);
        }
    }
    
    private void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.onExperienceChanged.RemoveListener(OnExperienceChanged);
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
        }
    }
}