using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject fade;
    // [SerializeField] GameObject menu;

    public static bool isMenuFlag;

    bool isSuccess;

    void Start()
    {
        fade.SetActive(true);               //フェードを有効化
                                            //  menu.SetActive(false);              //メニューを非表示
        isMenuFlag = false;                 //メニューフラグを無効化

        //ローカルのユーザーデータを取得
        // isSuccess = UserModel.Instance.LoadUserData();

        //ローカルのユーザーデータを取得
        // isSuccess = OptionModel.Instance.LoadOptionData();

        //BGM再生
        BGMManager.Instance.Play(
            audioPath: BGMPath.TITLE, //再生したいオーディオのパス
            volumeRate: 0.55f,                //音量の倍率
            delay: 1.0f,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: true,             //ループ再生するか
            allowsDuplicate: false             //他のBGMと重複して再生させるか
        );

        SEManager.Instance.Play(
           audioPath: SEPath.TITLE_WIND, //再生したいオーディオのパス
           volumeRate: 0.1f,                //音量の倍率
           delay: 0,                //再生されるまでの遅延時間
           pitch: 1,                //ピッチ
           isLoop: true,             //ループ再生するか
           callback: null              //再生終了後の処理
        );

        SEManager.Instance.Play(
           audioPath: SEPath.TITLE_NOISE, //再生したいオーディオのパス
           volumeRate: 0.1f,                //音量の倍率
           delay: 0,                //再生されるまでの遅延時間
           pitch: 1,                //ピッチ
           isLoop: true,             //ループ再生するか
           callback: null              //再生終了後の処理
        );

        //全てのBGMをフェードイン
        BGMManager.Instance.FadeIn(1.0f);
        //全てのSEをフェードイン
        SEManager.Instance.FadeIn(13.0f);
    }

    public void OpenOptionButton()
    {
        // menu.SetActive(true);
        isMenuFlag = true;

    }

    public void CloseOptionButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.SYSTEM20, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );

        isMenuFlag = false;
        Invoke("CloseMenu", 0.5f);

    }

    void CloseMenu()
    {
        // menu.SetActive(false);
    }

    public void OnClickStart()
    {

        //全てのBGMをフェードアウト
        BGMManager.Instance.FadeOut(BGMPath.TITLE, 3, () => {
            Debug.Log("BGMフェードアウト終了");
        });
        SEManager.Instance.FadeOut(SEPath.TITLE_WIND, 3, () => {
            Debug.Log("BGMフェードアウト終了");
        });
        SEManager.Instance.FadeOut(SEPath.TITLE_NOISE, 3, () => {
            Debug.Log("BGMフェードアウト終了");
        });
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
        SceneManager.LoadScene("PlayStandbyScene");
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
