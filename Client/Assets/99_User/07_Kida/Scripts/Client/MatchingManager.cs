////////////////////////////////////////////////////////////////
///
/// マッチング画面の処理を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class MatchingManager : MonoBehaviour
{
    UserModel userModel;
    [SerializeField] RoomModel roomModel;
    [SerializeField] GameObject userPrefab; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        userModel = GameObject.Find("UserModel").GetComponent<UserModel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public async void JoinRoom()
    {
        string roomName="Sample";
        await roomModel.JoinAsync(roomName, userModel.userId);


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
