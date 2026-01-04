using UnityEngine;

public class TestLevelUpUI : MonoBehaviour
{
    [SerializeField] private LevelUpUI levelUpUI;
    
    private void Start()
    {
        if (levelUpUI == null)
        {
            levelUpUI = FindObjectOfType<LevelUpUI>();
        }
        
        // 延迟3秒显示测试UI
        Invoke("ShowTestUI", 3f);
    }
    
    private void ShowTestUI()
    {
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpUI(5, 2, 5);
            Debug.Log("显示测试升级UI");
        }
    }
    
    [ContextMenu("测试升级UI")]
    public void TestLevelUp()
    {
        ShowTestUI();
    }
}