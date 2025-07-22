////////////////////////////////////////////////////////////////
///
/// マッチング画面の処理を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    UserModel userModel;                    //ユーザーModel
    [SerializeField] RoomModel roomModel;   //ルームの情報
    [SerializeField] GameObject userPrefab; //ユーザーの情報
    [SerializeField] Text inputFieldUserId; //ユーザーのID(デバッグ用)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //実装時にはこの変数でユーザーを判断する
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //接続処理
        await roomModel.ConnectAsync();
    }

    /// <summary>
    /// 入室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void JoinRoom()
    {

        int userId;
        int.TryParse(inputFieldUserId.text, out userId);
        string roomName="Sample";

        //RoomModelの入室同期を呼び出す
        await roomModel.JoinAsync(roomName,userId );


    }

    /// <summary>
    /// 入室完了処理
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //入室したときの処理を書く
        Debug.Log(joinedUser.UserData.Name + "が入室しました。");
    }
}
