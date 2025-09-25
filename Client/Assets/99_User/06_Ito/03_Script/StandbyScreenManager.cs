using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StandbyScreenManager : MonoBehaviour
{
    [SerializeField] Text setText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// �ҋ@��ʂ���}���`�I����ʂɈړ�
    /// </summary>
    public void BackOnlineButton()
    {
        //SceneManager.LoadScene("OnlineMultiScene");
        if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
        {
            Initiate.DoneFading();
            Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // �t�F�[�h����1�b
        }
        else
        {
            Initiate.DoneFading();
            Initiate.Fade("Title Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
        }
    }

    public void SetReady()
    {
        if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
        {
            setText.text = "��������";
        }
        else
        {
            setText.text = "GAME START";
        }
    }

    public void ChangeScene()
    {
        Initiate.DoneFading();
        Initiate.Fade("Stage Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    public void PushDifficultyButton(int id)
    {
        switch (id)
        {
            case 0:
                // ��Փxeasy�ɐݒ�
                break;
            case 1:
                // ��Փx�m�[�}���ɐݒ�
                break;
            case 2:
                // �n�[�h�ɐݒ�
                break;
        }
    }
}
