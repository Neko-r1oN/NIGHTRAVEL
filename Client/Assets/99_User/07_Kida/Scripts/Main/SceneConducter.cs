using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConducter : MonoBehaviour
{
    public void Loading()
    {
        Debug.Log("ロード中");
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);    // 現在のシーンにLoadingSceneシーンを追加する
    }    
    
    public void Loaded()
    {
        Debug.Log("ロード完了");
        SceneManager.UnloadSceneAsync("LoadingScene"); //LoadingSceneシーンを削除
    }    
    
    public void TakeYourPlayer()
    {
        Debug.Log("プレイヤー待ち");
        SceneManager.LoadScene("TakeYourPlayerScene", LoadSceneMode.Additive);    // 現在のシーンにTakeYourPlayerSceneシーンを追加する
    }
    
    public void SameStartPlayers()
    {
        Debug.Log("プレイヤー到着");
        SceneManager.UnloadSceneAsync("TakeYourPlayerScene"); //TakeYourPlayerSceneシーンを削除
        if (SceneManager.GetSceneByName("TakeYourPlayerScene").isLoaded)
            SceneManager.UnloadSceneAsync("TakeYourPlayerScene");
    }
}
