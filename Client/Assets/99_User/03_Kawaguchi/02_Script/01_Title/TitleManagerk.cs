using DG.Tweening;                   //DOTweenを使うときはこのusingを入れる
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks.Triggers;
using Grpc.Net.Client;
using MagicOnion.Client;
using NIGHTRAVEL.Shared.Interfaces.Services;
using Shared.Interfaces.StreamingHubs;
using UnityEngine.UI;
using System.Threading.Tasks;


public class TitleManagerk : MonoBehaviour
{
    [SerializeField] GameObject fade;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject roomModelPrefab;
    [SerializeField] GameObject license;
    [SerializeField] GameObject exitLicense;
    // [SerializeField] GameObject menu;

    //steamユーザー名
    private static string steamusername;
    public static string SteamUserName
    {
        get { return steamusername; }
    }

    //ゲームのモード
    //ソロモード 0,マルチプレイ 1,チュートリアル 2
    private static int gamemode;

    public static int GameMode
    {
        get { return gamemode; }
    }

    public static bool isMenuFlag;

    bool isSuccess;


    void Start()
    {

        //Steamのユーザー名を取得
        if (SteamManager.Initialized)
        {
            steamusername = SteamFriends.GetPersonaName();
            Debug.Log("Steam Username: " + steamusername);
            // 取得したユーザー名をゲーム内で表示する処理を記述
        }
        else
        {
            Debug.LogError("Steamworks is not initialized.");
        }

        fade.SetActive(true);               //フェードを有効化
      //  menu.SetActive(false);            //メニューを非表示
        isMenuFlag = false;                 //メニューフラグを無効化
        license.SetActive(false);           //ライセンスの画面を非表示
        exitLicense.SetActive(false);       //ライセンス画面を閉じるUIを非表示

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

        //ルームモデルがあるなら削除
        Destroy(GameObject.Find("RoomModel"));

        // 保持していた各データをリセットする
        CharacterManager.SelfPlayerStatusData = null;
        RelicManager.HaveRelicList = new List<RelicData>();
        LevelManager.GameLevel = 0;
        LevelManager.Options = new Dictionary<Guid, List<StatusUpgrateOptionData>>();

        Invoke("NewRoomModel", 0.1f);

    }

    void NewRoomModel()
    {
        if (GameObject.Find("RoomModel") != null) return;
        conducter.Loading();
        //ルームモデルをもう一度作成
        Instantiate(roomModelPrefab);
        Invoke("Loaded", 1.0f);
    }

    private void OnDisable()
    {
        RoomModel.Instance.OnCreatedRoom -= OnCreatedRoom;
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

    /// <summary>
    /// ソロモード
    /// </summary>
    public async void SinglePlayStart()
    {
        await RoomModel.Instance.ConnectAsync();
        RoomModel.Instance.OnCreatedRoom += OnCreatedRoom;
        gamemode = 0;
        await RoomModel.Instance.JoinedAsync("！！ソロモード専用のルームです！！", MatchingManager.UserID, SteamUserName, "404",gamemode);
    }

    /// <summary>
    /// ソロモード用ルーム作成通知
    /// </summary>
    public void OnCreatedRoom()
    {
        Initiate.DoneFading();
        Initiate.Fade("3_StandbyRoom", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// マルチプレイ
    /// </summary>
    public void MultiPlayStart()
    {
        gamemode = 1;
        Initiate.DoneFading();
        Initiate.Fade("2_MultiRoomScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// チュートリアル
    /// </summary>
    public void TutorialPlayStart()
    {
        gamemode = 2;
        Initiate.DoneFading();
        Initiate.Fade("Tutorial", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// ライセンス
    /// </summary>
    public void License()
    {
        license.SetActive(true);            //ライセンス画面を表示
        exitLicense.SetActive(true);        //ライセンス画面を閉じるUIを表示
    }

    /// <summary>
    /// ライセンスを閉じる
    /// </summary>
    public void ExitLicense()
    {
        license.SetActive(false);          //ライセンス画面を非表示
        exitLicense.SetActive(false);      //ライセンス画面を閉じるUIを非表示 
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Unityエディターでの動作
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 実際のゲーム終了処理
        Application.Quit();
#endif
    }

    private void Loaded()
    {
        conducter.Loaded();
    }
}

