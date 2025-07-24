////////////////////////////////////////////////////////////////
///
/// �}�b�`���O��ʂ̏������Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    UserModel userModel;                    //���[�U�[Model
    [SerializeField] RoomModel roomModel;   //���[���̏��
    [SerializeField] GameObject userPrefab; //���[�U�[�̏��
    [SerializeField] Text inputFieldRoomId; //���[����ID(�f�o�b�O�p)
    [SerializeField] Text inputFieldUserId; //���[�U�[��ID(�f�o�b�O�p)
    JoinedUser joinedUser;                  //���̃N���C�A���g���[�U�[�̏��


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //�������ɂ͂��̕ϐ��Ń��[�U�[�𔻒f����
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //�ڑ�����
        await roomModel.ConnectAsync();
        //���[�U�[��������������OnJoinedUser���\�b�h�����s����悤�A���f���ɓo�^
        roomModel.OnJoinedUser += this.OnJoinedUser;
        //���[�U�[���ގ���������OnLeavedUser���\�b�h�����s����悤�A���f���ɓo�^
        roomModel.OnLeavedUser += this.OnLeavedUser;
        //���[�U�[������������������OnReady���\�b�h�����s����悤�A���f���ɓo�^
        roomModel.OnReadySyn += this.OnReadySyn;
    }

    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void JoinRoom()
    {

        int userId;
        int.TryParse(inputFieldUserId.text, out userId);
        string roomName=inputFieldRoomId.text;
        
        //RoomModel�̓����������Ăяo��
        await roomModel.JoinAsync(roomName,userId );


    }

    /// <summary>
    /// �ގ�����
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void LeaveRoom()
    {
        //RoomHub�̑ގ��������Ăяo��
        await roomModel.LeaveAsync();
    }

    /// <summary>
    /// ������������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void Ready()
    {
        await roomModel.ReadyAsync();
    }

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //���������Ƃ��̏���������
        Debug.Log(joinedUser.UserData.Name + "���������܂����B");
    }

    /// <summary>
    /// �ގ������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnLeavedUser(JoinedUser joinedUser)
    {
        //�ގ������Ƃ��̏���������
        Debug.Log(joinedUser.UserData.Name + "���ގ����܂����B");
    }

    /// <summary>
    /// ���������ʒm
    /// </summary>
    public void OnReadySyn(Guid guid)
    {

        //�������������Ƃ��̏���������
        Debug.Log( guid.ToString() + "���������I�I");
    }
}
