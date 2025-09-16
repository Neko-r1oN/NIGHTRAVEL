////////////////////////////////////////////////////////////////
///
/// マッチング画面の処理を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

#region using一覧
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks.Triggers;
using Grpc.Net.Client;
using MagicOnion.Client;
using NIGHTRAVEL.Shared.Interfaces.Services;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion

public class MatchingManager : MonoBehaviour
{
    #region [SerializeField]：木田晃輔
    //[SerializeField] GameObject userPrefab; //ユーザーの情報
    [SerializeField] Text inputFieldRoomId; //ルームのID(デバッグ用)
    //[SerializeField] Text inputFieldUserId; //ユーザーのID(デバッグ用)
    //[SerializeField] Text inputFieldCharacterId; //キャラID(デバッグ用)
    [SerializeField] int userId;//新マッチング用のユーザーID
    [SerializeField] int characterId;//新マッチング用のキャラクターID
    [SerializeField] GameObject roomPrefab; //ルームのプレハブ
    [SerializeField] Text roomNameText; //ルームのプレハブ
    [SerializeField] Text userNameText; //ルームのプレハブ
    [SerializeField] GameObject Content;
    #endregion

    UserModel userModel;                    //ユーザーModel
    JoinedUser joinedUser;                  //このクライアントユーザーの情報
    Text text;
    BaseModel model;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //実装時にはこの変数でユーザーを判断する
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        #region RoomModel定義
        //接続処理
        await RoomModel.Instance.ConnectAsync();
        RoomModel.Instance.OnSearchedRoom += this.OnSearchedRoom;
        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
        //ユーザーが退室した時にOnLeavedUserメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnLeavedUser += this.OnLeavedUser;
        //ユーザーが準備完了した時にOnReadyメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnReadySyn += this.OnReadySyn;
        //ゲーム開始が出来る状態の時にメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnStartedGame += this.OnStartedGame;
        //ゲーム開始が出来る状態の時にメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;
        #endregion

        SerchRoom();
       
    }

    private void OnDisable()
    {
        //シーン遷移した場合に通知関数をモデルから解除
        RoomModel.Instance.OnSearchedRoom -= this.OnSearchedRoom;
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;
    }

    public async void SerchRoom()
    {
        await RoomModel.Instance.SearchRoomAsync();
    }

    #region 同期処理一覧：木田晃輔
    /// <summary>
    /// 入室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void JoinRoom()
    {

        //int userId;
        //int.TryParse(inputFieldUserId.text, out userId);
        string roomName=inputFieldRoomId.text;
        
        //RoomModel.Instanceの入室同期を呼び出す
        await RoomModel.Instance.JoinedAsync(roomName,userId );


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
    #endregion

    #region 通知一覧：木田晃輔
    /// <summary>
    /// 検索通知
    ///  Aughter:木田晃輔
    /// </summary>
    public void OnSearchedRoom(List<string> roomNameList, List<string> userNameList)
    {
        //ループ用変数
        int i = 0;

        foreach(var roomData in RoomModel.Instance.roomDataList)
        {
            //RoomDataに格納する
            roomData.roomName = roomNameList[i];
            roomData.userName = userNameList[i];

            i++;
        }

        foreach (var roomData in RoomModel.Instance.roomDataList)
        {
            //ルームを表示させる
            GameObject searchedRoom = Instantiate(roomPrefab,Content.transform);

            //子オブジェクトを探す
            GameObject roomN = searchedRoom.transform.Find("RoomName").gameObject;
            GameObject userN = searchedRoom.transform.Find("UserName").gameObject;

            //ルーム名を表示させる
            roomNameText = roomN.GetComponent<Text>();
            roomNameText.text = roomData.roomName;

            //ホスト名を表示させる
            userNameText = userN.GetComponent<Text>();
            userNameText.text = roomData.userName;
        }
    }

    /// <summary>
    /// 入室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        foreach (var data in RoomModel.Instance.joinedUserList.Values)
        {
            //入室したときの処理を書く
            Debug.Log(data.UserData.Name + "が入室しました。");

        }     
    }

    /// <summary>
    /// 退室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnLeavedUser(JoinedUser joinedUser)
    {
        //退室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が退室しました。");
    }

    /// <summary>
    /// 準備完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnReadySyn(Guid guid)
    {
        //準備完了したときの処理を書く
        Debug.Log( guid.ToString() + "準備完了！！");


    }

    /// <summary>
    /// ゲーム開始通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnStartedGame()
    {
        //ゲーム開始の時の処理を書く
        Debug.Log("ゲームを開始します");
        SceneManager.LoadScene("Stage Kida");
    }

    public void OnChangedMasterClient()
    {
        Debug.Log("あなたがマスタークライアントになりました。");
        RoomModel.Instance.IsMaster = true;
        //SpawnManager.Instance.SpawnCnt = CharacterManager.Instance.Enemies.Count;
    }
    #endregion
}
