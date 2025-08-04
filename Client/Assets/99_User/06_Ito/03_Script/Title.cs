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
    /// �^�C�g������\���v���C�Ɉړ�
    /// </summary>
    public void SinglePlayButton()
    {
        //SceneManager.LoadScene("SinglePlayScene");
        Initiate.Fade("SinglePlayScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// �^�C�g������I�����C���}���`�Ɉړ�
    /// </summary>
    public void OnlineButton()
    {
        //SceneManager.LoadScene("OnlineMultiScene");
        Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// �^�C�g������f�[�^���X�g�Ɉړ�
    /// </summary>
    public void DataListButton()
    {
        //SceneManager.LoadScene("DataListScene");
        Initiate.Fade("DataListScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// �^�C�g������I�v�V�����ݒ�Ɉړ�
    /// </summary>
    public void SettingButton()
    {
        //SceneManager.LoadScene("SettingScene");
        Initiate.Fade("SettingScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }
}
