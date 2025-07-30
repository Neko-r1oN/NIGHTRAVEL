////////////////////////////////////////////////////////////////
///
/// �}�b�`���O��ʂ̏������Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

#region using�ꗗ
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion

public class MatchingManager : MonoBehaviour
{
    #region [SerializeField]�F�ؓc�W��
    [SerializeField] GameObject userPrefab; //���[�U�[�̏��
    [SerializeField] Text inputFieldRoomId; //���[����ID(�f�o�b�O�p)
    [SerializeField] Text inputFieldUserId; //���[�U�[��ID(�f�o�b�O�p)
    #endregion
    UserModel userModel;                    //���[�U�[Model
    JoinedUser joinedUser;                  //���̃N���C�A���g���[�U�[�̏��


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //�������ɂ͂��̕ϐ��Ń��[�U�[�𔻒f����
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        #region RoomModel��`
        //�ڑ�����
        await RoomModel.Instance.ConnectAsync();
        //���[�U�[��������������OnJoinedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnJoinedUser += this.OnJoinedUser;
        //���[�U�[���ގ���������OnLeavedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnLeavedUser += this.OnLeavedUser;
        //���[�U�[������������������OnReady���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnReadySyn += this.OnReadySyn;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnStartedGame += this.OnStartedGame;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;
        #endregion
    }

    private void OnDisable()
    {
        //�V�[���J�ڂ����ꍇ�ɒʒm�֐������f���������
        RoomModel.Instance.OnJoinedUser -= this.OnJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;
    }

    #region ���������ꗗ�F�ؓc�W��
    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void JoinRoom()
    {

        int userId;
        int.TryParse(inputFieldUserId.text, out userId);
        string roomName=inputFieldRoomId.text;
        
        //RoomModel.Instance�̓����������Ăяo��
        await RoomModel.Instance.JoinAsync(roomName,userId );


    }

    /// <summary>
    /// �ގ�����
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void LeaveRoom()
    {
        //RoomHub�̑ގ��������Ăяo��
        await RoomModel.Instance.LeaveAsync();
    }

    /// <summary>
    /// ������������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void Ready()
    {
        await RoomModel.Instance.ReadyAsync();
    }
    #endregion

    #region �ʒm�ꗗ�F�ؓc�W��
    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        foreach(var data in RoomModel.Instance.joinedUserList.Values)
        {
            //���������Ƃ��̏���������
            Debug.Log(data.UserData.Name + "���������܂����B");
        }
        
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
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnReadySyn(Guid guid)
    {
        //�������������Ƃ��̏���������
        Debug.Log( guid.ToString() + "���������I�I");


    }

    /// <summary>
    /// �Q�[���J�n�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnStartedGame()
    {
        //�Q�[���J�n�̎��̏���������
        Debug.Log("�Q�[�����J�n���܂�");
        SceneManager.LoadScene("01_Stage");
    }

    public void OnChangedMasterClient()
    {
        Debug.Log("���Ȃ����}�X�^�[�N���C�A���g�ɂȂ�܂����B");
    }
    #endregion
}
