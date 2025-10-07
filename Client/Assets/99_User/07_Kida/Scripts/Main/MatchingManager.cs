////////////////////////////////////////////////////////////////
///
/// �}�b�`���O��ʂ̏������Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

#region using�ꗗ
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
    #region [SerializeField]�F�ؓc�W��
    //[SerializeField] GameObject userPrefab; //���[�U�[�̏��
    [SerializeField] Text inputFieldRoomName; //���[���̖��O���̓t�B�[���h
    [SerializeField] Text inputFieldSerchRoomName; //���[���̖��O���̓t�B�[���h
    [SerializeField] Text inputFieldCreatePassWord; //�p�X���[�h�̍쐬�t�B�[���h
    [SerializeField] Text inputFieldPassWord; //�p�X���[�h�̓��̓t�B�[���h
    [SerializeField] Text roomSerchField;   //���[���̖��O����
    [SerializeField] GameObject roomPrefab; //���[���̃v���n�u
    [SerializeField] GameObject Content;
    [SerializeField] Transform rooms;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject CreateButton; //�����{�^��
    [SerializeField] GameObject PrivateUI;
    [SerializeField] GameObject[] ErrorUI;
    [SerializeField] GameObject fade;
    #endregion
    public List<GameObject> createdRoomList; //���ꂽ���[��
    EventSystem eventSystem;
    UserModel userModel;                    //���[�U�[Model
    JoinedUser joinedUser;                  //���̃N���C�A���g���[�U�[�̏��
    Text text;
    BaseModel model;
    Text roomNameText; //���[���̖��O
    Text userNameText; //���[�U�[�̖��O
    Text passText;      //�p�X���[�h
    string joinRoomName;
    string roomSerchName;
    int errorId;

    //�V�}�b�`���O�p�̃��[�U�[ID
    private static int userId;
    public static int UserID
    {
        get { return userId; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //�������ɂ͂��̕ϐ��Ń��[�U�[�𔻒f����
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        #region RoomModel��`
        //�ڑ�����
        await RoomModel.Instance.ConnectAsync();
        RoomModel.Instance.OnSearchedRoom += this.OnSearchedRoom;
        RoomModel.Instance.OnCreatedRoom += this.OnCreatedRoom;
        RoomModel.Instance.OnFailedJoinSyn += this.OnFailedJoinSyn;
        //���[�U�[��������������OnJoinedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
        #endregion

        SerchRoom();
       
    }

    private void OnDisable()
    {
        //�V�[���J�ڂ����ꍇ�ɒʒm�֐������f���������
        RoomModel.Instance.OnSearchedRoom -= this.OnSearchedRoom;
        RoomModel.Instance.OnCreatedRoom -= this.OnCreatedRoom;
        RoomModel.Instance.OnFailedJoinSyn -= this.OnFailedJoinSyn;
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
    }

    public void ReturnTitle()
    {
        Initiate.DoneFading();
        Initiate.Fade("1_TitleScene", Color.black, 1.0f);   // �t�F�[�h����1�b
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

    #region ���������ꗗ�F�ؓc�W��

    /// <summary>
    /// ���[���쐬
    /// </summary>
    public async void CreateRoom()
    {
        conducter.Loading();

        if (Re_RoomManager.IsCreater == true)
        {//���[���쐬�̏ꍇ
            passText = inputFieldCreatePassWord;
            roomNameText = inputFieldRoomName;
            await RoomModel.Instance.JoinedAsync(roomNameText.text, userId,TitleManagerk.SteamUserName, passText.text);
        }
    }

    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void JoinRoom(string roomName , bool isPass)
    {
        joinRoomName = roomName;

        if (isPass == true)
        {
            //�p�X���[�h���̓p�l���I�[�v��
            PrivateUI.SetActive(true);
        }
        else
        {
            conducter.Loading();
            await RoomModel.Instance.JoinedAsync(joinRoomName, userId,TitleManagerk.SteamUserName, "");
        }
    }

    /// <summary>
    /// �v���C�x�[�g���[������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void PrivateRoomJoin()
    {
        conducter.Loading();
        string pass = inputFieldPassWord.text;
        await RoomModel.Instance.JoinedAsync(joinRoomName, userId,TitleManagerk.SteamUserName, pass);
    }

    #endregion

    #region �ʒm�ꗗ�F�ؓc�W��
    /// <summary>
    /// �����ʒm
    ///  Aughter:�ؓc�W��
    /// </summary>
    public void OnSearchedRoom(List<string> roomNameList, List<string> userNameList,List<string> passWordList)
    {

        foreach(var room in createdRoomList)
        {
            GameObject.Destroy(room);
        }

        //���[�v�p�ϐ�
        int i = 0;

        foreach(var roomData in RoomModel.Instance.roomDataList)
        {
            //RoomData�Ɋi�[����
            roomData.roomName = roomNameList[i];
            roomData.userName = userNameList[i];
            roomData.passWord = passWordList[i];

   
            //���[����\��������
            GameObject newGamaObj = Instantiate(roomPrefab);
            newGamaObj.transform.parent = rooms;

            //�q�I�u�W�F�N�g��T��
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

            //���[������\��������
            roomNameText = roomN.GetComponent<Text>();
            roomNameText.text = roomData.roomName;

            //�z�X�g����\��������
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
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        foreach (var data in RoomModel.Instance.joinedUserList.Values)
        {
            //���������Ƃ��̏���������
            Debug.Log(data.UserData.Name + "���������܂����B");

        }     
    }
    #endregion
}
