using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
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
    /// タイトルからソロプレイに移動
    /// </summary>
    public void SinglePlayButton()
    {
        //SceneManager.LoadScene("SinglePlayScene");
        Initiate.Fade("SinglePlayScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// タイトルからオンラインマルチに移動
    /// </summary>
    public void OnlineButton()
    {
        //SceneManager.LoadScene("OnlineMultiScene");
        Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// タイトルからデータリストに移動
    /// </summary>
    public void DataListButton()
    {
        //SceneManager.LoadScene("DataListScene");
        Initiate.Fade("DataListScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// タイトルからオプション設定に移動
    /// </summary>
    public void SettingButton()
    {
        //SceneManager.LoadScene("SettingScene");
        Initiate.Fade("SettingScene", Color.black, 1.0f);   // フェード時間1秒
    }
}
