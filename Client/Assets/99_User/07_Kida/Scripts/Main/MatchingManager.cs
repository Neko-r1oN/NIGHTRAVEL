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
#endregion

public class MatchingManager : MonoBehaviour
{
    #region [SerializeField]：木田晃輔
    //[SerializeField] GameObject userPrefab; //ユーザーの情報
    [SerializeField] Text inputFieldRoomName; //ルームの名前入力フィールド
    [SerializeField] Text inputFieldSerchRoomName; //ルームの名前入力フィールド
    [SerializeField] Text inputFieldCreatePassWord; //パスワードの作成フィールド
    [SerializeField] Text inputFieldPassWord; //パスワードの入力フィールド
    [SerializeField] Text roomSerchField;   //ルームの名前検索
    [SerializeField] GameObject roomPrefab; //ルームのプレハブ
    [SerializeField] GameObject Content;
    [SerializeField] Transform rooms;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject CreateButton; //生成ボタン
    [SerializeField] GameObject PrivateUI;
    [SerializeField] GameObject[] ErrorUI;
    [SerializeField] GameObject fade;
    #endregion
    public List<GameObject> createdRoomList; //作られたルーム
    EventSystem eventSystem;
    UserModel userModel;                    //ユーザーModel
    JoinedUser joinedUser;                  //このクライアントユーザーの情報
    Text text;
    BaseModel model;
    Text roomNameText; //ルームの名前
    Text userNameText; //ユーザーの名前
    Text passText;      //パスワード
    string joinRoomName;
    string roomSerchName;
    int errorId;

    //新マッチング用のユーザーID
    private static int userId;
    public static int UserID
    {
        get { return userId; }
    }

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
        RoomModel.Instance.OnFailedJoinSyn += this.OnFailedJoinSyn;
        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
        #endregion

        SerchRoom();
       
    }

    private void OnDisable()
    {
        //シーン遷移した場合に通知関数をモデルから解除
        RoomModel.Instance.OnSearchedRoom -= this.OnSearchedRoom;
        RoomModel.Instance.OnCreatedRoom -= this.OnCreatedRoom;
        RoomModel.Instance.OnFailedJoinSyn -= this.OnFailedJoinSyn;
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
    }

    public void ReturnTitle()
    {
        Initiate.DoneFading();
        Initiate.Fade("1_TitleScene", Color.black, 1.0f);   // フェード時間1秒
    }

    public void ErrorClose()
    {
        ErrorUI[errorId].SetActive(false);
    }

    public async void SerchRoom()
    {
        conducter.Loading();
        //roomSerchName = inputFieldSerchRoomName.text;
        await RoomModel.Instance.SearchRoomAsync();
    }

    private void Loaded()
    {
        conducter.Loaded();
    }

    #region 同期処理一覧：木田晃輔

    /// <summary>
    /// ルーム作成
    /// </summary>
    public async void CreateRoom()
    {
        conducter.Loading();

        if (Re_RoomManager.IsCreater == true)
        {//ルーム作成の場合
            passText = inputFieldCreatePassWord;
            roomNameText = inputFieldRoomName;
            await RoomModel.Instance.JoinedAsync(roomNameText.text, userId,TitleManagerk.SteamUserName, passText.text);
        }
    }

    /// <summary>
    /// 入室処理
    /// Aughter:木田晃輔
    /// </summary>
    public async void JoinRoom(string roomName , bool isPass)
    {
        joinRoomName = roomName;

        if (isPass == true)
        {
            //パスワード入力パネルオープン
            PrivateUI.SetActive(true);
        }
        else
        {
            conducter.Loading();
            await RoomModel.Instance.JoinedAsync(joinRoomName, userId,TitleManagerk.SteamUserName, "");
        }
    }

    /// <summary>
    /// プライベートルーム入室
    /// Aughter:木田晃輔
    /// </summary>
    public async void PrivateRoomJoin()
    {
        conducter.Loading();
        string pass = inputFieldPassWord.text;
        await RoomModel.Instance.JoinedAsync(joinRoomName, userId,TitleManagerk.SteamUserName, pass);
    }

    #endregion

    #region 通知一覧：木田晃輔
    /// <summary>
    /// 検索通知
    ///  Aughter:木田晃輔
    /// </summary>
    public void OnSearchedRoom(List<string> roomNameList, List<string> userNameList,List<string> passWordList)
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
            roomData.passWord = passWordList[i];

   
            //ルームを表示させる
            GameObject newGamaObj = Instantiate(roomPrefab);
            newGamaObj.transform.parent = rooms;

            //子オブジェクトを探す
            GameObject roomN = newGamaObj.transform.Find("RoomName").gameObject;
            GameObject userN = newGamaObj.transform.Find("UserName").gameObject;
            GameObject button = newGamaObj.transform.Find("Button(Join) (1)").gameObject;

            if(roomData.passWord == "")
            {
                newGamaObj.transform.Find("Private").gameObject.SetActive(false);

                button.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomData.roomName,false));
            }
            else
            {
                newGamaObj.transform.Find("Private").gameObject.SetActive(true);

                button.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomData.roomName, true));
            }

            //ルーム名を表示させる
            roomNameText = roomN.GetComponent<Text>();
            roomNameText.text = roomData.roomName;

            //ホスト名を表示させる
            userNameText = userN.GetComponent<Text>();
            userNameText.text = roomData.userName;

            createdRoomList.Add(newGamaObj);

            i++;

        }
        Invoke("Loaded", 1.0f);
    }

    public void OnFailedJoinSyn(int errorId)
    {
        this.errorId = errorId;
        if(this.errorId == 0) 
        {
            ErrorUI[this.errorId].SetActive(true);
            conducter.Loaded();
        }
        if(this.errorId == 1)
        {
            PrivateUI.SetActive(false);
            ErrorUI[this.errorId].SetActive(true);
            conducter.Loaded();
        }
    }

    public void OnCreatedRoom()
    {
        SceneManager.LoadScene("3_StandbyRoom");
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
    #endregion
}
