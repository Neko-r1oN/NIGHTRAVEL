using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
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
    /// �ݒ��ʂ���^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    }
}
