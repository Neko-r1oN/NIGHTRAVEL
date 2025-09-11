using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;                   //DOTweenを使うときはこのusingを入れる
using KanKikuchi.AudioManager;

public class TitleManagerk : MonoBehaviour
{

    [SerializeField] GameObject fade;
   // [SerializeField] GameObject menu;

    public static bool isMenuFlag;

    bool isSuccess;

    void Start()
    {
        //全てのBGMをフェードアウト
        BGMManager.Instance.FadeIn(1.0f);
        //全てのBGMをフェードアウト
        SEManager.Instance.FadeIn(1.0f);

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

    void MuteSound()
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
}

