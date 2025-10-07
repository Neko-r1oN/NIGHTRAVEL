using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.ShaderGraph;
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
    }


    private void OnDisable()
    {
        RoomModel.Instance.OnJoinedUser -= OnJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;
        RoomModel.Instance.OnChangedMasterClient -= this.OnChangedMasterClient;
    }

    public void UpdatePlayerIcon()
    {
        foreach(var icon in playerIcons)
        {
            Destroy(icon);
        }
        playerIcons.Clear();
        //初期化
        //GameObject playerN = new GameObject();
        //GameObject userN = new GameObject();
        int i = 0;

        foreach (var joinedUser in RoomModel.Instance.joinedUserList)
        {
            GameObject gameObject = Instantiate(playerIconPrefab);
            gameObject.transform.parent = playerIconsZone;

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
    }

    public async void ReturnMaching()
    {
        await RoomModel.Instance.LeavedAsync();
        Initiate.DoneFading();
        Initiate.Fade("2_MultiRoomScene", Color.black, 1.0f);   // フェード時間1秒
    }

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
        //キャラクターを送る
        int character = characterId;
        await RoomModel.Instance.ReadyAsync(character);
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
        Text gameObject = Instantiate(logTextPrefab);
        gameObject.text = joinedUser.UserData.Name + "が入室しました。";
        gameObject.transform.parent = logs.transform;
        gameObject.transform.position = logs.transform.position;
        logList.Add(gameObject.gameObject);
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
        playerIcons[RoomModel.Instance.joinedUserList[guid].JoinOrder-1].GetComponent<Image>().color =new Color(255.0f,183.0f,0.0f);
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
    public void OnStartedGame()
    { 
        //ゲーム開始の時の処理を書く
        conducter.Loading();
        Debug.Log("ゲームを開始します");
        //SceneManager.LoadScene("4_Stage_01");
        SceneManager.LoadScene("4_Stage_01");
        Invoke("Loaded", 1.0f);

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
