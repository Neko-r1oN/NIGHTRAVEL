using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class StandbyManager : MonoBehaviour
{
    [SerializeField] int characterId;//新マッチング用のキャラクターID
    [SerializeField] GameObject[] characters;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject fade;
    [SerializeField] GameObject[] characterImage;
    [SerializeField] GameObject readyButton;
    [SerializeField] Text characterNameText;
    [SerializeField] GameObject playerIconPrefab;
    [SerializeField] Transform playerIconsZone;
    [SerializeField] Image[] iconImages;
    [SerializeField] Sprite[] iconCharacterImage;
    [SerializeField] Text logTextPrefab;
    [SerializeField] GameObject logs;
    [SerializeField] List<GameObject> logList;

    public List<GameObject> playerIcons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RoomModel.Instance.OnJoinedUser += OnJoinedUser;
        //ユーザーが退室した時にOnLeavedUserメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnLeavedUser += this.OnLeavedUser;
        //ユーザーがキャラクター変更をした場合に
        RoomModel.Instance.OnChangedCharacter += this.OnChangeIcon;
        //ユーザーが準備完了した時にOnReadyメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnReadySyn += this.OnReadySyn;
        //ゲーム開始が出来る状態の時にメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnStartedGame += this.OnStartedGame;
        //マスタークライアント譲渡
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;


        //アイコンを更新
        Loading();
        Invoke("UpdatePlayerIcon", 1.0f);
        Invoke("Loaded", 3.0f);

        //ログの自動削除
        InvokeRepeating("LogDelite", 5.0f,5.0f);
    }


    private void OnDisable()
    {
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;
        RoomModel.Instance.OnChangedMasterClient -= this.OnChangedMasterClient;
    }

    /// <summary>
    /// プレイヤーアイコンの更新
    ///  Aughter:木田晃輔
    /// </summary>
    public void UpdatePlayerIcon()
    {
        //foreach(var icon in playerIcons)
        //{
        //    Destroy(icon);
        //}

        //playerIcons.Clear();

        int i = 0;

        foreach (var joinedUser in RoomModel.Instance.joinedUserList)
        {
            GameObject gameObject = playerIcons[i];

            //子オブジェクトを探す
            GameObject playerN = gameObject.transform.Find("Number").gameObject;
            GameObject userN = gameObject.transform.Find("name").gameObject;

            Text playerNText = playerN.GetComponent<Text>();
            Text userNText = userN.GetComponent<Text>();

            
            playerNText.text = RoomModel.Instance.joinedUserList[joinedUser.Key].JoinOrder.ToString() + "P";
            userNText.text = RoomModel.Instance.joinedUserList[joinedUser.Key].UserData.Name;
            

            playerIcons.Add(gameObject);
            iconImages[i] = gameObject.transform.Find("IconBG").gameObject.transform.Find("Icon").gameObject.GetComponent<Image>();
            iconImages[i].sprite = iconCharacterImage[joinedUser.Value.CharacterID];
            i++;
        }

        //ソロプレイの場合は下を行わない
        if (TitleManagerk.GameMode == 0)
        {
            Destroy(playerIcons[1]);
            Destroy(playerIcons[2]);
            return;
        } 

        if(i < 2)
        {
            for(int j=1;j<3;j++)
            {
                GameObject gameObject = playerIcons[i];

                //子オブジェクトを探す
                GameObject playerN = gameObject.transform.Find("Number").gameObject;
                GameObject userN = gameObject.transform.Find("name").gameObject;

                Text playerNText = playerN.GetComponent<Text>();
                Text userNText = userN.GetComponent<Text>();

                i++;
                playerNText.text = i.ToString() + "P";
                userNText.text = "募集中";


                playerIcons.Add(gameObject);
                iconImages[j] = gameObject.transform.Find("IconBG").gameObject.transform.Find("Icon").gameObject.GetComponent<Image>();
                iconImages[j].sprite = iconCharacterImage[0];
            }
        }
        if (i < 3)
        {
            GameObject gameObject = playerIcons[i];

            //子オブジェクトを探す
            GameObject playerN = gameObject.transform.Find("Number").gameObject;
            GameObject userN = gameObject.transform.Find("name").gameObject;

            Text playerNText = playerN.GetComponent<Text>();
            Text userNText = userN.GetComponent<Text>();


            playerNText.text = "3P";
            userNText.text = "検索中";


            playerIcons.Add(gameObject);
            iconImages[i] = gameObject.transform.Find("IconBG").gameObject.transform.Find("Icon").gameObject.GetComponent<Image>();
            iconImages[i].sprite = iconCharacterImage[0];
            
        }

    }

    /// <summary>
    /// マッチングシーン遷移
    ///  Aughter:木田晃輔
    /// </summary>
    public async void ReturnMaching()
    {
        await RoomModel.Instance.LeavedAsync();
        Initiate.DoneFading();
        if (TitleManagerk.GameMode == 0) 
        {//ソロ
            Initiate.Fade("1_TitleScene", Color.black, 1.0f);   // フェード時間1秒
        }
        else if (TitleManagerk.GameMode == 1)
        {//マルチプレイ
            Initiate.Fade("2_MultiRoomScene", Color.black, 1.0f);   // フェード時間1秒
        }
    }

    /// <summary>
    /// キャラクターを変更
    ///  Aughter:木田晃輔
    /// </summary>
    /// <param name="changeCharacterId"></param>
    public async void ChangeCharacter(int changeCharacterId)
    {
        readyButton.SetActive(true);                                //準備完了ボタンを表示
        characterImage[characterId].SetActive(false);               //前のキャラレビュー非表示
        characterImage[changeCharacterId].SetActive(true);          //新しいキャラレビューを表示
        this.characterId = changeCharacterId;                       //キャラクターＩＤを最新に
        characterNameText.text = characterImage[characterId].name;  //キャラクター名を最新に

        if (RoomModel.Instance)
            await RoomModel.Instance.ChangeCharacterAsync(this.characterId);
        else
            ChangeIcon(this.characterId);
    }

    /// <summary>
    /// アイコン表示
    /// 準備完了した際に押されたキャラのボタンのIDからアイコンの画像を変更する感じ
    /// 使う変数 
    /// Image[] iconImages (アイコンのイメージ)
    /// Material[] iconCharacterImage (貼り付けたい画像)
    /// 例：iconImages[自分のID].material = iconCharacterImage[changeIconId]
    /// </summary>
    /// <param name="changeIconId"></param>
    public void ChangeIcon(int changeIconId)
    {
        iconImages[MatchingManager.UserID].sprite = iconCharacterImage[changeIconId];
    }

    /// <summary>
    /// アイコン表示通知
    ///  Aughter:木田晃輔
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="changeIconId"></param>
    public void OnChangeIcon( Guid guid,int changeIconId)
    {
        iconImages[RoomModel.Instance.joinedUserList[guid].JoinOrder-1].sprite = iconCharacterImage[changeIconId];
    }

    private void Loading()
    {
        conducter.Loading();
    }


    private void Loaded()
    {
        conducter.Loaded();
    }

    private void LogDelite()
    {
        if (!RoomModel.Instance || logList.Count == 0) return;
        GameObject gameObject = logList[0];
        logList.Remove(gameObject);
        Destroy(gameObject);
    }


    /// <summary>
    /// 退室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void LeaveRoom()
    {
        //RoomHubの退室同期を呼び出す
        await RoomModel.Instance.LeavedAsync();
    }

    /// <summary>
    /// 準備完了処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void Ready()
    {
        if(TitleManagerk.GameMode==0)
        {//ソロプレイの場合
            //キャラクターを送る
            int character = characterId;
            readyButton.SetActive(false);
            await RoomModel.Instance.ReadyAsync(character);
        }
        else
        {//マルチプレイ
            if (RoomModel.Instance.joinedUserList.Count < 2) return;
            int character = characterId;
            readyButton.SetActive(false);
            await RoomModel.Instance.ReadyAsync(character);
        }
    }


    /// <summary>
    /// 待機画面からマルチ選択画面に移動
    /// </summary>
    //public void BackOnlineButton()
    //{
    //    //SceneManager.LoadScene("OnlineMultiScene");
    //    if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
    //    {
    //        Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // フェード時間1秒
    //    }
    //    else
    //    {
    //        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // フェード時間1秒
    //    }
    //}

    //public void SetReady()
    //{
    //    if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
    //    {
    //        setText.text = "準備完了";
    //    }
    //    else
    //    {
    //        setText.text = "GAME START";
    //    }
    //}

    //public void ChangeScene()
    //{
    //    Initiate.Fade("Stage Ueno", Color.black, 1.0f);   // フェード時間1秒
    //}

    /// <summary>
    /// 入室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //入室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が入室しました。");

        //ログの設定
        Text gameObject = Instantiate(logTextPrefab);
        gameObject.text = joinedUser.UserData.Name + "が入室しました。";
        gameObject.transform.parent = logs.transform;
        gameObject.transform.position = logs.transform.position;
        logList.Add(gameObject.gameObject);

        //アイコン更新
        UpdatePlayerIcon();
    }

    /// <summary>
    /// 退室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnLeavedUser(JoinedUser joinedUser)
    {
        //退室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が退室しました。");

        //ログの生成
        Text gameObject = Instantiate(logTextPrefab);
        gameObject.text = joinedUser.UserData.Name + "が退室しました。";
        gameObject.transform.parent = logs.transform;
        gameObject.transform.position = logs.transform.position;
        logList.Add(gameObject.gameObject);
        UpdatePlayerIcon();
    }

    /// <summary>
    /// 準備完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnReadySyn(Guid guid)
    {
        //準備完了したときの処理を書く
        Debug.Log(guid.ToString() + "準備完了！！");

        //アイコンを準備完了が分かるように色を変える
        playerIcons[RoomModel.Instance.joinedUserList[guid].JoinOrder-1].GetComponent<Image>().color =new Color(255.0f,183.0f,0.0f);

        //ソロプレイの場合下の処理をしない
        if (TitleManagerk.GameMode == 0) return;

        //ログの生成
        Text gameObject = Instantiate(logTextPrefab);
        gameObject.text = RoomModel.Instance.joinedUserList[guid].UserData.Name + "準備完了！！";
        gameObject.transform.parent = logs.transform;
        gameObject.transform.position = logs.transform.position;
        logList.Add(gameObject.gameObject);
    }

    /// <summary>
    /// ゲーム開始通知
    /// Aughter:木田晃輔
    /// </summary>
    public async void OnStartedGame()
    { 
        readyButton.SetActive(false);
        //ゲーム開始の時の処理を書く
        //conducter.Loading();
        Debug.Log("ゲームを開始します");
        if(RoomModel.Instance.IsMaster == true)
        {//ホストのみ
            await RoomModel.Instance.StartRoomAsync(TitleManagerk.SteamUserName);
        }
        //SceneManager.LoadScene("4_Stage_01");
        Initiate.DoneFading();
        Initiate.Fade("4_Stage_01", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// マスタークライアント譲渡通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnChangedMasterClient()
    {
        Debug.Log("あなたがルームのホストになりました。");
    }
}
