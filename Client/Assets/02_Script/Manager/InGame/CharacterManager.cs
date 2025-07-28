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
using DG.Tweening;
using UnityEditor.MemoryProfiler;
using UnityEngine.Playables;

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

    /// <summary>
    /// �v���C���[�̃��X�g
    /// </summary>
    public Dictionary<Guid, GameObject> PlayerObjs { get {  return playerObjs; } }
    #endregion

    #region �G�֘A
    Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();

    /// <summary>
    /// ���݂̃X�e�[�W�Ő��������G�̃��X�g
    /// </summary>
    public Dictionary<int, GameObject> Enemies { get { return enemies; } }
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

    /// <summary>
    /// �V�[���J�ڂ����Ƃ��ɒʒm�֐��Ăяo�����~�߂�
    /// </summary>
    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnMovePlayerSyn -= this.OnMovePlayer;                    //�V�[���J�ڂ����ꍇ�ɒʒm�֐������f���������
        RoomModel.Instance.OnUpdateMasterClientSyn -= this.OnUpdateMasterClient;    //�V�[���J�ڂ����ꍇ�ɒʒm�֐������f���������
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

        if (!RoomModel.Instance || RoomModel.Instance.ConnectionId == Guid.Empty) 
        {
            playerObjs.Add(Guid.Empty, playerObjSelf);
            return;
        }
        DestroyExistingPlayer();
        GenerateCharacters();
        isAwake = true;

        //�v���C���[�̍X�V�ʒm���ɌĂ�
        RoomModel.Instance.OnMovePlayerSyn += this.OnMovePlayer;
        //�}�X�^�[�N���C�A���g�̍X�V�ʒm���ɌĂ�
        RoomModel.Instance.OnUpdateMasterClientSyn += this.OnUpdateMasterClient;
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        StartCoroutine("UpdateCoroutine");
    }

    /// <summary>
    /// �L�����N�^�[�̏��X�V�Ăяo���p�R���[�`��
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (RoomModel.Instance.IsMaster) UpdateMasterDataRequest();
            else UpdatePlayerDataRequest();
            yield return new WaitForSeconds(updateSec);
        }
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
    /// �V���ȓG�����X�g�ɒǉ�����
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemies(int id, GameObject newEnemies)
    {
        enemies.Add(id, newEnemies);
    }

    /// <summary>
    /// �L�����N�^�[�̏����X�V����
    /// </summary>
    /// <param name="characterData"></param>
    /// <param name="character"></param>
    void UpdateCharacter(CharacterData characterData, CharacterBase character)
    {
        var statusData = new CharacterStatusData(
            characterData.Health,
            characterData.Defense,
            characterData.AttackPower,
            characterData.MoveSpeed,
            characterData.MoveSpeedFactor,
            characterData.AttackSpeedFactor,
            characterData.JumpPower
            );

        character.OverridCurrentStatus(statusData);
        character.gameObject.SetActive(characterData.IsActiveSelf);
        character.gameObject.transform.DOMove(characterData.Position, updateSec).SetEase(Ease.Linear);
        character.gameObject.transform.localScale = characterData.Scale;
        character.gameObject.transform.DORotateQuaternion(characterData.Rotation, updateSec).SetEase(Ease.Linear);
        character.SetAnimId(characterData.AnimationId);
        character.gameObject.GetComponent<StatusEffectController>().ApplyStatusEffect(false, characterData.DebuffList);

        // �}�X�^�[�N���C�A���g�̏ꍇ�A�L�����N�^�[��������悤�ɂ���
        if (RoomModel.Instance.IsMaster && !character.enabled)
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            character.enabled = true;
        }
    }

    /// <summary>
    /// �v���C���[���擾
    /// </summary>
    /// <returns></returns>
    PlayerData GetPlayerData()
    {
        if (!playerObjs.ContainsKey(RoomModel.Instance.ConnectionId)) return null;
        Debug.Log("�L�����N�^�[�F"+RoomModel.Instance.ConnectionId);
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
            Scale = player.transform.localScale, 
            Rotation = player.transform.rotation,
            AnimationId = player.GetAnimId(),
            DebuffList = statusEffectController.GetAppliedStatusEffects(),

            // �ȉ��͐�p�ϐ�
            PlayerID = 0,   // ######################################################### �Ƃ肠����0�Œ�
            ConnectionId = RoomModel.Instance.ConnectionId,
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
                Scale = enemy.transform.localScale,
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
    async void UpdateMasterDataRequest()
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
    async void UpdatePlayerDataRequest()
    {
        var playerData = GetPlayerData();

        // �v���C���[���X�V���N�G�X�g
        await RoomModel.Instance.UpdatePlayerAsync(playerData);
    }

    /// <summary>
    /// �v���C���[�̍X�V�̒ʒm
    /// </summary>
    /// <param name="playerData"></param>
    void OnMovePlayer(PlayerData playerData)
    {
        if (!playerObjs.ContainsKey(playerData.ConnectionId)) return;

        // �v���C���[�̏��X�V
        var player = playerObjs[playerData.ConnectionId].GetComponent<PlayerBase>();
        UpdateCharacter(playerData, player);
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̍X�V�ʒm
    /// </summary>
    /// <param name="masterClientData"></param>
    void OnUpdateMasterClient(MasterClientData masterClientData)
    {
        if (!playerObjs.ContainsKey(masterClientData.PlayerData.ConnectionId)) return;

        // �v���C���[�̏��X�V
        var player = playerObjs[masterClientData.PlayerData.ConnectionId].GetComponent<PlayerBase>();
        UpdateCharacter(masterClientData.PlayerData, player);

        // �G�̏��X�V
        foreach(var enemyData in masterClientData.EnemyDatas)
        {
            if (enemies.ContainsKey(enemyData.EnemyID) && enemies[enemyData.EnemyID] != null)
            {
                var enemy = enemies[enemyData.EnemyID].GetComponent<EnemyBase>();
                UpdateCharacter(enemyData, enemy);
            }
        }
    }
}
