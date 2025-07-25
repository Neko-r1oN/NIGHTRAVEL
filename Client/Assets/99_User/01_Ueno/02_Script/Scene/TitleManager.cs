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
    /// �^�C�g������\���v���C�Ɉړ�
    /// </summary>
    public void SinglePlayButton()
    {
        SceneManager.LoadScene("SinglePlayScene");
    }

    /// <summary>
    /// �^�C�g������I�����C���}���`�Ɉړ�
    /// </summary>
    public void OnlineButton()
    {
        SceneManager.LoadScene("OnlineMultiScene");
    }

    /// <summary>
    /// �^�C�g������f�[�^���X�g�Ɉړ�
    /// </summary>
    public void DataListButton()
    {
        SceneManager.LoadScene("DataListScene");
    }

    /// <summary>
    /// �^�C�g������I�v�V�����ݒ�Ɉړ�
    /// </summary>
    public void SettingButton()
    {
        SceneManager.LoadScene("SettingScene");
    }
}
