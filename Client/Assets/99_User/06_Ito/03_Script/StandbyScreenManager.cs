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
    /// 待機画面からマルチ選択画面に移動
    /// </summary>
    public void BackOnlineButton()
    {
        //SceneManager.LoadScene("OnlineMultiScene");
        if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
        {
            Initiate.DoneFading();
            Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // フェード時間1秒
        }
        else
        {
            Initiate.DoneFading();
            Initiate.Fade("Title Ueno", Color.black, 1.0f);   // フェード時間1秒
        }
    }

    public void SetReady()
    {
        if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
        {
            setText.text = "準備完了";
        }
        else
        {
            setText.text = "GAME START";
        }
    }

    public void ChangeScene()
    {
        Initiate.DoneFading();
        Initiate.Fade("Stage Ueno", Color.black, 1.0f);   // フェード時間1秒
    }

    public void PushDifficultyButton(int id)
    {
        switch (id)
        {
            case 0:
                // 難易度easyに設定
                break;
            case 1:
                // 難易度ノーマルに設定
                break;
            case 2:
                // ハードに設定
                break;
        }
    }
}
