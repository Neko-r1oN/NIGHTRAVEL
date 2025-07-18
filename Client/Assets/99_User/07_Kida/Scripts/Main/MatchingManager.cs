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
    UserModel userModel;
    [SerializeField] RoomModel roomModel;
    [SerializeField] GameObject userPrefab;
    [SerializeField] Text inputFieldRoomId;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //接続処理
        await roomModel.ConnectAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 入室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void JoinRoom()
    {

        int roomId;
        int.TryParse(inputFieldRoomId.text, out roomId);
        string roomName="Sample";
        await roomModel.JoinAsync(roomName,roomId );


    }

    /// <summary>
    /// 入室完了処理
    /// Aughter:木田晃輔
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {

        Debug.Log(joinedUser.UserData.Name + "が入室しました。");
    }
}
