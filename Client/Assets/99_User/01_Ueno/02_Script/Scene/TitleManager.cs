using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject fade;

    public static bool isMenuFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject obj = GameObject.Find("UISet");
        Destroy(obj);

        fade.SetActive(true);               //フェードを有効化
        //  menu.SetActive(false);              //メニューを非表示
        isMenuFlag = false;                 //メニューフラグを無効化
    }

    public void OpenOptionButton()
    {
        // menu.SetActive(true);
        isMenuFlag = true;
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
