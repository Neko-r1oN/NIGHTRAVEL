using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

public class PreGameManager : MonoBehaviour
{
    [SerializeField] RoomModel roomModel;           //RoomModel�N���X���g�p
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //���[�U�[���ړ���������OnMoveUser���\�b�h�����s����悤�A���f���ɓo�^
        roomModel.OnMovePlayerSyn += this.OnMoveCharacterSyn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// �v���C���[�̈ړ�����
    /// </summary>
    public async�@void PlayerUpdate()
    {
        
    }

    /// <summary>
    /// �v���C���[�̈ړ��ʒm
    /// </summary>
    public void OnMoveCharacterSyn(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID)
    {

    }

    /// <summary>
    /// �G�̈ړ�����
    /// </summary>
    void EnemyUpdate()
    {

    }

}
