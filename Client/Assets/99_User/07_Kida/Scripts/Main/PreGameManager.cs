using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

public class PreGameManager : MonoBehaviour
{
    [SerializeField] RoomModel roomModel;           //RoomModel�N���X���g�p
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;

    //���[�U�[ID�ƃQ�[���I�u�W�F�N�g�𓯎��Ɋi�[
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    int anim = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //���[�U�[��������������OnJoinedUser���\�b�h�����s����悤�A���f���ɓo�^(�f�o�b�O�p)
        roomModel.OnJoinedUser += this.OnJoinedUser;
        //���[�U�[���ގ���������OnLeavedUser���\�b�h�����s����悤�A���f���ɓo�^(�f�o�b�O�p)
        roomModel.OnLeavedUser += this.OnLeavedUser;
        //���[�U�[���ړ���������OnMoveUser���\�b�h�����s����悤�A���f���ɓo�^
        //roomModel.OnMovePlayerSyn += this.OnMoveCharacterSyn;
        //�G���ړ���������OnMoveUser���\�b�h�����s����悤�A���f���ɓo�^
        roomModel.OnMoveEnemySyn += this.OnMoveEnemySyn;
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
        //await roomModel.MovePlayerAsync(playerData);
    }

    /// <summary>
    /// �G�̈ړ�����
    /// </summary>
    void EnemyUpdate()
    {

    }

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        //���������Ƃ��̏���������
        Debug.Log(joinedUser.UserData.Name + "���������܂����B");

        //�����ł���΃v���C���[���o��������
        if(roomModel.ConnectionId==joinedUser.ConnectionId)
        {
            Instantiate(player);
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

        //�ގ������v���C���[������
        player.SetActive(false);
    }

    /// <summary>
    /// �v���C���[�̈ړ��ʒm
    /// </summary>
    public void OnMoveCharacterSyn(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID)
    {

    }

    /// <summary>
    /// �G�̈ړ��ʒm
    /// </summary>
    public void OnMoveEnemySyn(int enemID, Vector2 pos, Quaternion rot, EnemyAnimState anim)
    {

    }

}
