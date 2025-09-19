using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);    // 現在のシーンにUIManagerシーンを追加する
    }

    /// <summary>
    /// 指定した名前のシーンを再読み込みします。
    /// </summary>
    /// <param name="sceneName">再読み込みしたいシーンの名前</param>
    public void ReloadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
