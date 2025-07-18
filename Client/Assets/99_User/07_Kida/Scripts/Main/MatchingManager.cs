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
    UserModel userModel;
    [SerializeField] RoomModel roomModel;
    [SerializeField] GameObject userPrefab;
    [SerializeField] Text inputFieldRoomId;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //userModel = GameObject.Find("UserModel").GetComponent<UserModel>();

        //�ڑ�����
        await roomModel.ConnectAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void JoinRoom()
    {

        int roomId;
        int.TryParse(inputFieldRoomId.text, out roomId);
        string roomName="Sample";
        await roomModel.JoinAsync(roomName,roomId );


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
