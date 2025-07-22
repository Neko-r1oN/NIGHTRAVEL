////////////////////////////////////////////////////////////////
///
/// �}�b�`���O��ʂ̏������Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    UserModel userModel;                    //���[�U�[Model
    [SerializeField] RoomModel roomModel;   //���[���̏��
    [SerializeField] GameObject userPrefab; //���[�U�[�̏��
    [SerializeField] Text inputFieldUserId; //���[�U�[��ID(�f�o�b�O�p)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //�������ɂ͂��̕ϐ��Ń��[�U�[�𔻒f����
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //�ڑ�����
        await roomModel.ConnectAsync();
    }

    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void JoinRoom()
    {

        int userId;
        int.TryParse(inputFieldUserId.text, out userId);
        string roomName="Sample";

        //RoomModel�̓����������Ăяo��
        await roomModel.JoinAsync(roomName,userId );


    }

    /// <summary>
    /// ������������
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //���������Ƃ��̏���������
        Debug.Log(joinedUser.UserData.Name + "���������܂����B");
    }
}
