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
using static Unity.Cinemachine.CinemachineSplineRoll;
#endregion

public class MatchingManager : MonoBehaviour
{
    #region [SerializeField]�F�ؓc�W��
    //[SerializeField] GameObject userPrefab; //���[�U�[�̏��
    [SerializeField] Text inputFieldRoomId; //���[����ID(�f�o�b�O�p)
    [SerializeField] Text roomSerchField;   //���[���̖��O����
    //[SerializeField] Text inputFieldUserId; //���[�U�[��ID(�f�o�b�O�p)
    //[SerializeField] Text inputFieldCharacterId; //�L����ID(�f�o�b�O�p)
    [SerializeField] int userId;//�V�}�b�`���O�p�̃��[�U�[ID
    [SerializeField] GameObject roomPrefab; //���[���̃v���n�u
    [SerializeField] Text roomNameText; //���[���̃v���n�u
    [SerializeField] Text userNameText; //���[���̃v���n�u
    [SerializeField] GameObject Content;
    [SerializeField] Transform rooms;
    [SerializeField] GameObject CreateButton; //�����{�^��
    public List<GameObject> createdRoomList; //���ꂽ���[��
    EventSystem eventSystem;
    #endregion

    UserModel userModel;                    //���[�U�[Model
    JoinedUser joinedUser;                  //���̃N���C�A���g���[�U�[�̏��
    Text text;
    BaseModel model;


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
        //���[�U�[��������������OnJoinedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;
        #endregion

        SerchRoom();
       
    }

    private void OnDisable()
    {
        //�V�[���J�ڂ����ꍇ�ɒʒm�֐������f���������
        RoomModel.Instance.OnSearchedRoom -= this.OnSearchedRoom;
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
    }

    public async void SerchRoom()
    {
        await RoomModel.Instance.SearchRoomAsync();
    }

    #region ���������ꗗ�F�ؓc�W��
    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
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

    #region �ʒm�ꗗ�F�ؓc�W��
    /// <summary>
    /// �����ʒm
    ///  Aughter:�ؓc�W��
    /// </summary>
    public void OnSearchedRoom(List<string> roomNameList, List<string> userNameList)
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

   
            //���[����\��������
            GameObject newGamaObj = Instantiate(roomPrefab);
            newGamaObj.transform.parent = rooms;

            //�q�I�u�W�F�N�g��T��
            GameObject roomN = newGamaObj.transform.Find("RoomName").gameObject;
            GameObject userN = newGamaObj.transform.Find("UserName").gameObject;
            GameObject button = newGamaObj.transform.Find("Button(Join) (1)").gameObject;

            button.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomData.roomName));

            //���[������\��������
            roomNameText = roomN.GetComponent<Text>();
            roomNameText.text = roomData.roomName;

            //�z�X�g����\��������
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

    public void OnChangedMasterClient()
    {
        Debug.Log("���Ȃ����}�X�^�[�N���C�A���g�ɂȂ�܂����B");
        RoomModel.Instance.IsMaster = true;
        //SpawnManager.Instance.SpawnCnt = CharacterManager.Instance.Enemies.Count;
    }
    #endregion
}
