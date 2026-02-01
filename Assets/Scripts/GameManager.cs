using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏管理器 - 单例模式
/// 负责处理游戏结束逻辑和全局状态管理
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 单例模式 (Singleton)
    
    private static GameManager instance;
    
    /// <summary>
    /// 全局访问点
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("GameManager: 场景中没有找到 GameManager 实例！");
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // 确保只有一个实例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // 可选：跨场景保持（如果需要的话）
        // DontDestroyOnLoad(gameObject);
    }
    
    #endregion
    
    #region 游戏结束逻辑
    
    [Header("=== 游戏结束 UI ===")]
    public GameObject endScreenImage;  // 感谢图片/结束画面
    
    /// <summary>
    /// 显示游戏结束画面
    /// </summary>
    public void ShowGameEnd()
    {
        // 打印日志
        Debug.Log("游戏结束");
        
        // 显示结束画面
        if (endScreenImage != null)
        {
            endScreenImage.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager: endScreenImage 未设置！");
        }
        
        // 暂停游戏时间（让整个世界静止）
        Time.timeScale = 0f;
    }
    
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 重置游戏时间（用于重新开始游戏）
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 复活/重试：恢复时间、隐藏结束画面、重载当前场景。
    /// 用于死亡界面“复活/重试”按钮。
    /// </summary>
    public void Respawn()
    {
        ResumeGame();
        if (endScreenImage != null)
            endScreenImage.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 返回主菜单：恢复时间、隐藏结束画面、加载指定场景。
    /// </summary>
    /// <param name="menuSceneName">主菜单场景名，默认 "scene0"</param>
    public void ReturnToMenu(string menuSceneName = "scene0")
    {
        ResumeGame();
        if (endScreenImage != null)
            endScreenImage.SetActive(false);
        SceneManager.LoadScene(menuSceneName);
    }
    
    private void OnDestroy()
    {
        // 清理时恢复时间流速
        if (instance == this)
        {
            Time.timeScale = 1f;
            instance = null;
        }
    }
    
    #endregion
}
