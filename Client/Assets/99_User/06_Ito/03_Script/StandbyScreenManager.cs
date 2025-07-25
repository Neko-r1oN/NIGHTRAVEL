using UnityEngine;
using UnityEngine.SceneManagement;

public class StandbyScreenManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 待機画面からマルチ選択画面に移動
    /// </summary>
    public void BackOnlineButton()
    {
        SceneManager.LoadScene("OnlineMultiScene");
    }
}
