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

    // タイトルからSinglePlaytSceneに移動
    public void SinglePlayButton()
    {
        SceneManager.LoadScene("SinglePlayScene");
    }

    // タイトルからOnlinetSceneに移動
    public void OnlineButton()
    {
        SceneManager.LoadScene("lineScene");
    }

    // タイトルからDataListSceneに移動
    public void DataListButton()
    {
        SceneManager.LoadScene("DataListScene");
    }

    // タイトルからSettingSceneに移動
    public void SettingButton()
    {
        SceneManager.LoadScene("SettingScene");
    }
}
