using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
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
    /// ���U���g��ʂ���^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���U���g��ʂ��烍�r�[�Ɉړ�
    /// </summary>
    public void BackLobbySceneButton()
    {
        Initiate.Fade("PlayStandbyScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }
}
