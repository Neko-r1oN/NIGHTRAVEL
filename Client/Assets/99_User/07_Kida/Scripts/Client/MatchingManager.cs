////////////////////////////////////////////////////////////////
///
/// �}�b�`���O��ʂ̏������Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
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
    /// ������������
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {

        Debug.Log(joinedUser.UserData.Name + "���������܂����B");
    }
}
