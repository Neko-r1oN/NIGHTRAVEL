////////////////////////////////////////////////////////////////
///
/// マッチング画面の処理を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    UserModel userModel;                    //ユーザーModel
    [SerializeField] RoomModel roomModel;   //ルームの情報
    [SerializeField] GameObject userPrefab; //ユーザーの情報
    [SerializeField] Text inputFieldRoomId; //ルームのID(デバッグ用)
    [SerializeField] Text inputFieldUserId; //ユーザーのID(デバッグ用)
    JoinedUser joinedUser;                  //このクライアントユーザーの情報


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //実装時にはこの変数でユーザーを判断する
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //接続処理
        await roomModel.ConnectAsync();
        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録
        roomModel.OnJoinedUser += this.OnJoinedUser;
        //ユーザーが退室した時にOnLeavedUserメソッドを実行するよう、モデルに登録
        roomModel.OnLeavedUser += this.OnLeavedUser;
        //ユーザーが準備完了した時にOnReadyメソッドを実行するよう、モデルに登録
        roomModel.OnReadySyn += this.OnReadySyn;
    }

    /// <summary>
    /// 入室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void JoinRoom()
    {

        int userId;
        int.TryParse(inputFieldUserId.text, out userId);
        string roomName=inputFieldRoomId.text;
        
        //RoomModelの入室同期を呼び出す
        await roomModel.JoinAsync(roomName,userId );


    }

    /// <summary>
    /// 退室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void LeaveRoom()
    {
        //RoomHubの退室同期を呼び出す
        await roomModel.LeaveAsync();
    }

    /// <summary>
    /// 準備完了処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void Ready()
    {
        await roomModel.ReadyAsync();
    }

    /// <summary>
    /// 入室完了通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //入室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が入室しました。");
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
    /// </summary>
    public void OnReadySyn(Guid guid)
    {

        //準備完了したときの処理を書く
        Debug.Log( guid.ToString() + "準備完了！！");
    }
}
