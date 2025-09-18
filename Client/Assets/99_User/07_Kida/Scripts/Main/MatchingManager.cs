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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Cinemachine.CinemachineSplineRoll;
#endregion

public class MatchingManager : MonoBehaviour
{
    #region [SerializeField]：木田晃輔
    //[SerializeField] GameObject userPrefab; //ユーザーの情報
    [SerializeField] Text inputFieldRoomId; //ルームのID(デバッグ用)
    [SerializeField] Text roomSerchField;   //ルームの名前検索
    //[SerializeField] Text inputFieldUserId; //ユーザーのID(デバッグ用)
    //[SerializeField] Text inputFieldCharacterId; //キャラID(デバッグ用)
    [SerializeField] int userId;//新マッチング用のユーザーID
    [SerializeField] GameObject roomPrefab; //ルームのプレハブ
    [SerializeField] Text roomNameText; //ルームのプレハブ
    [SerializeField] Text userNameText; //ルームのプレハブ
    [SerializeField] GameObject Content;
    [SerializeField] Transform rooms;
    [SerializeField] GameObject CreateButton; //生成ボタン
    public List<GameObject> createdRoomList; //作られたルーム
    EventSystem eventSystem;
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
        RoomModel.Instance.OnCreatedRoom += this.OnCreatedRoom;
        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
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
    public async void JoinRoom(string roomName)
    {
        if(Re_RoomManager.IsCreater == true)
        {
            roomNameText = inputFieldRoomId;
            await RoomModel.Instance.JoinedAsync(roomNameText.text, userId);
        }
        else
        {
            await RoomModel.Instance.JoinedAsync(roomName, userId);
        }
    }

    #endregion

    #region 通知一覧：木田晃輔
    /// <summary>
    /// 検索通知
    ///  Aughter:木田晃輔
    /// </summary>
    public void OnSearchedRoom(List<string> roomNameList, List<string> userNameList)
    {

        foreach(var room in createdRoomList)
        {
            GameObject.Destroy(room);
        }

        //ループ用変数
        int i = 0;

        foreach(var roomData in RoomModel.Instance.roomDataList)
        {
            //RoomDataに格納する
            roomData.roomName = roomNameList[i];
            roomData.userName = userNameList[i];

   
            //ルームを表示させる
            GameObject newGamaObj = Instantiate(roomPrefab);
            newGamaObj.transform.parent = rooms;

            //子オブジェクトを探す
            GameObject roomN = newGamaObj.transform.Find("RoomName").gameObject;
            GameObject userN = newGamaObj.transform.Find("UserName").gameObject;
            GameObject button = newGamaObj.transform.Find("Button(Join) (1)").gameObject;

            button.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomData.roomName));

            //ルーム名を表示させる
            roomNameText = roomN.GetComponent<Text>();
            roomNameText.text = roomData.roomName;

            //ホスト名を表示させる
            userNameText = userN.GetComponent<Text>();
            userNameText.text = roomData.userName;

            createdRoomList.Add(newGamaObj);

            i++;

        }
    }

    public void OnCreatedRoom()
    {
        SceneManager.LoadScene("Standby_Kida");
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

    public void OnChangedMasterClient()
    {
        Debug.Log("あなたがマスタークライアントになりました。");
        RoomModel.Instance.IsMaster = true;
        //SpawnManager.Instance.SpawnCnt = CharacterManager.Instance.Enemies.Count;
    }
    #endregion
}
