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

    // �^�C�g������SinglePlaytScene�Ɉړ�
    public void SinglePlayButton()
    {
        SceneManager.LoadScene("SinglePlayScene");
    }

    // �^�C�g������OnlinetScene�Ɉړ�
    public void OnlineButton()
    {
        SceneManager.LoadScene("lineScene");
    }

    // �^�C�g������DataListScene�Ɉړ�
    public void DataListButton()
    {
        SceneManager.LoadScene("DataListScene");
    }

    // �^�C�g������SettingScene�Ɉړ�
    public void SettingButton()
    {
        SceneManager.LoadScene("SettingScene");
    }
}
