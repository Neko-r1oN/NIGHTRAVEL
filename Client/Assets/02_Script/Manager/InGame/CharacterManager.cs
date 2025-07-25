//**************************************************
//  ���݂��Ă���L�����N�^�[�̊Ǘ����s��
//  Author:r-enomoto
//**************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared;
using Shared.Interfaces.StreamingHubs;
using System.Drawing;

public class CharacterManager : MonoBehaviour
{
    #region �v���C���[�֘A
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // �e�v���C���[�̏����ʒu
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject playerObjSelf;  // ���[�J���p�ɑ����t�^
    Dictionary<Guid, GameObject> playerObjs = new Dictionary<Guid, GameObject>();

    /// <summary>
    /// �����̑���L����
    /// </summary>
    public GameObject PlayerObjSelf { get { return playerObjSelf; } }
    #endregion

    #region �G�֘A
    Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
    #endregion

    const float updateSec = 0.1f;
    bool isAwake = false;

    static CharacterManager instance;
    public static CharacterManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
        if (RoomModel.Instance.ConnectionId == Guid.Empty) return;
        DestroyExistingPlayer();
        GenerateCharacters();
        StartCoroutine("UpdateCoroutine");
        isAwake = true;
    }

    /// <summary>
    /// ���ɃV�[����ɑ��݂��Ă���v���C���[��j������
    /// </summary>
    void DestroyExistingPlayer()
    {
        // ���ɃV�[����ɑ��݂��Ă��鑀��L�������폜����
        var players = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            Destroy(player);
        }
    }

    /// <summary>
    /// �Q�����Ă��郆�[�U�[�������ɁA�v���C���[�𐶐�����
    /// </summary>
    void GenerateCharacters()
    {
        foreach (var key in RoomModel.Instance.joinedUserList.Keys)
        {
            var point = startPoints[0];
            startPoints.RemoveAt(0);

            var playerObj = Instantiate(charaSwordPrefab, point.position, Quaternion.identity);
            playerObjs.Add(key, playerObj);

            if (key == RoomModel.Instance.ConnectionId) playerObjSelf = playerObj;
        }
    }

    /// <summary>
    /// �L�����N�^�[�̏��X�V�Ăяo���p�R���[�`��
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        if (RoomModel.Instance.IsMaster) UpdateMasterData();
        else UpdatePlayerData();
        yield return new WaitForSeconds(updateSec);
    }

    /// <summary>
    /// �v���C���[���擾
    /// </summary>
    /// <returns></returns>
    PlayerData GetPlayerData()
    {
        var player = playerObjs[RoomModel.Instance.ConnectionId].GetComponent<PlayerBase>();
        var statusEffectController = player.GetComponent<StatusEffectController>();
        return new PlayerData()
        {
            IsActiveSelf = player.gameObject.activeInHierarchy,
            Health = player.HP,
            Defense = player.Defense,
            AttackPower = player.power,
            JumpPower = player.jumpPower,
            MoveSpeed = player.moveSpeed,
            MoveSpeedFactor = player.moveSpeedFactor,
            AttackSpeedFactor = player.attackSpeedFactor,
            Position = player.transform.position,
            Rotation = player.transform.rotation,
            AnimationId = player.GetAnimId(),
            DebuffList = statusEffectController.GetAppliedStatusEffects(),

            // �ȉ��͐�p�ϐ�
            PlayerID = 0,   // ######################################################### �Ƃ肠����0�Œ�
            IsDead = false
        };
    }

    /// <summary>
    /// �G�̏��擾
    /// </summary>
    /// <returns></returns>
    List<EnemyData> GetEnemyDatas()
    {
        List <EnemyData> enemyDatas = new List <EnemyData>();
        foreach (var key in enemies.Keys)
        {
            var enemyObj = enemies[key];
            var enemy = enemyObj.GetComponent<EnemyBase>();
            var statusEffectController = enemyObj.GetComponent<StatusEffectController>();
            var data = new EnemyData()
            {
                IsActiveSelf = enemy.gameObject.activeInHierarchy,
                Health = enemy.HP,
                Defense = enemy.Defense,
                AttackPower = enemy.power,
                JumpPower = enemy.jumpPower,
                MoveSpeed = enemy.moveSpeed,
                MoveSpeedFactor = enemy.moveSpeedFactor,
                AttackSpeedFactor = enemy.attackSpeedFactor,
                Position = enemy.transform.position,
                Rotation = enemy.transform.rotation,
                AnimationId = enemy.GetAnimId(),
                DebuffList = statusEffectController.GetAppliedStatusEffects(),

                // �ȉ��͐�p�ϐ�
                EnemyID = key,
                EnemyName = enemyObj.name,
                isBoss = enemy.IsBoss,
            };
        }
        return enemyDatas;
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�p�̏��X�V
    /// </summary>
    async void UpdateMasterData()
    {
        var masterClientData = new MasterClientData()
        {
            PlayerData = GetPlayerData(),
            EnemyDatas = GetEnemyDatas(),
        };

        // �}�X�^�N���C�A���g���X�V���N�G�X�g
        await RoomModel.Instance.UpdateMasterClientAsync(masterClientData);
    }

    /// <summary>
    /// �v���C���[�̏��X�V
    /// </summary>
    async void UpdatePlayerData()
    {
        var playerData = GetPlayerData();

        // �v���C���[���X�V���N�G�X�g
        await RoomModel.Instance.MovePlayerAsync(playerData);
    }

    /// <summary>
    /// �V���ȓG�����X�g�ɒǉ�����
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemies(params GameObject[] newEnemies)
    {
        foreach (GameObject enemy in newEnemies)
        {
            enemies.Add(enemies.Count, enemy);
        }
    }

    /// <summary>
    /// �v���C���[�̍X�V�̒ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerData"></param>
    void OnMovePlayer(PlayerData playerData)
    {
        
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̍X�V�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="masterClientData"></param>
    void OnUpdateMasterClient(MasterClientData masterClientData)
    {

    }
}
