using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConducter : MonoBehaviour
{
    public void Loading()
    {
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);    // 現在のシーンにLoadingSceneシーンを追加する
    }    
    
    public void Loaded()
    {
        SceneManager.UnloadSceneAsync("LoadingScene"); //LoadingSceneシーンを削除
    }
}
