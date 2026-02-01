using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        // 方便你在编辑器里测试“退出”的效果（可选）
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
