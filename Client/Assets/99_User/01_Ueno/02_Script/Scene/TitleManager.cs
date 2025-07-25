using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject obj = GameObject.Find("UISet");

        Destroy(obj);
    }

    public void ButtonPush()
    {
        SceneManager.LoadScene("Stage");
    }

    /// <summary>
    /// タイトルからソロプレイに移動
    /// </summary>
    public void SinglePlayButton()
    {
        SceneManager.LoadScene("SinglePlayScene");
    }

    /// <summary>
    /// タイトルからオンラインマルチに移動
    /// </summary>
    public void OnlineButton()
    {
        SceneManager.LoadScene("OnlineMultiScene");
    }

    /// <summary>
    /// タイトルからデータリストに移動
    /// </summary>
    public void DataListButton()
    {
        SceneManager.LoadScene("DataListScene");
    }

    /// <summary>
    /// タイトルからオプション設定に移動
    /// </summary>
    public void SettingButton()
    {
        SceneManager.LoadScene("SettingScene");
    }
}
