//**************************************************
//  ���݂��Ă���L�����N�^�[�̊Ǘ����s��
//  Author:r-enomoto
//**************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;
using System.Drawing;
using DG.Tweening;
using UnityEngine.Playables;
using System.Linq;
using System.Data;
using Unity.VisualScripting;

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
    public Dictionary<Guid, GameObject> PlayerObjs { get { return playerObjs; } }
    #endregion

    #region �G�֘A
    Dictionary<int, SpawnedEnemy> enemies = new Dictionary<int, SpawnedEnemy>();

    /// <summary>
    /// ���݂̃X�e�[�W�Ő��������G�̃��X�g
    /// </summary>
    public Dictionary<int, SpawnedEnemy> Enemies { get { return enemies; } }
    #endregion

    const float updateSec = 0.1f;

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

        if (!RoomModel.Instance || RoomModel.Instance.ConnectionId == Guid.Empty)
        {
            if (!playerObjSelf)
            {
                Debug.LogError("playerObjSelf���ݒ肳��Ă��Ȃ�");
            }
            playerObjs.Add(Guid.Empty, playerObjSelf);
            return;
        }
        DestroyExistingPlayer();
        GenerateCharacters();

        // �ʒm������o�^
        RoomModel.Instance.OnUpdatePlayerSyn += this.OnUpdatePlayer;
        RoomModel.Instance.OnUpdateMasterClientSyn += this.OnUpdateMasterClient;
        RoomModel.Instance.OnLeavedUser += this.OnLeave;
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        StartCoroutine("UpdateCoroutine");
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        StopAllCoroutines();

        // �V�[���J�ڂ����Ƃ��ɓo�^�����ʒm����������
        RoomModel.Instance.OnUpdatePlayerSyn -= this.OnUpdatePlayer;
        RoomModel.Instance.OnUpdateMasterClientSyn -= this.OnUpdateMasterClient;
        RoomModel.Instance.OnLeavedUser -= this.OnLeave;
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
            Destroy(player.gameObject);
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

            if (key == RoomModel.Instance.ConnectionId)
            {
                playerObjSelf = playerObj;
                Camera.main.gameObject.GetComponent<CameraFollow>().Target = playerObjSelf.transform;
            }
        }
    }

    /// <summary>
    /// �V���ȓG�����X�g�ɒǉ�����
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemiesToList(params SpawnedEnemy[] newEnemies)
    {
        foreach (var enemy in newEnemies)
        {
            enemies.Add(enemy.UniqueId, enemy);
        }
    }

    /// <summary>
    /// ���X�g����G���폜
    /// </summary>
    public void RemoveEnemyFromList(int uniqueId)
    {
        enemies.Remove(uniqueId);
    }

    /// <summary>
    /// SPAWN_ENEMY_TYPE�̒l�ɊY������G�����Ԃ�
    /// </summary>
    /// <param name="spawnType"></param>
    /// <returns></returns>
    public List<GameObject> GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE spawnType)
    {
        List<int> removeKeys = new List<int>();             // ���S�ς݂̓G��key
        List<GameObject> result = new List<GameObject>();   // �Ԃ��G�̃��X�g
        foreach (var data in enemies)
        {
            if (data.Value.Enemy.HP <= 0 || data.Value.Object == null) removeKeys.Add(data.Key);
            else if (data.Value.SpawnType == spawnType) result.Add(data.Value.Object);
        }
        foreach (var key in removeKeys)
        {
            enemies.Remove(key);
        }
        return result;
    }

    /// <summary>
    /// �L�����N�^�[�̏����X�V����
    /// </summary>
    /// <param name="characterData"></param>
    /// <param name="character"></param>
    void UpdateCharacter(CharacterData characterData, CharacterBase character)
    {
        var stateData = characterData.State;

        List<STATUS_TYPE> addStatusTypes = new List<STATUS_TYPE>() { STATUS_TYPE.All};
        if (character.gameObject.tag == "Enemy")
        {
            // �G�̏ꍇ��HP�ȊO���X�V����
            addStatusTypes = new List<STATUS_TYPE>() { 
                STATUS_TYPE.Defense, 
                STATUS_TYPE.JumpPower, 
                STATUS_TYPE.MoveSpeed, 
                STATUS_TYPE.AttackSpeedFactor};
        }
        character.OverridCurrentStatus(stateData, STATUS_TYPE.All);
        character.OverridCurrentStatus(stateData);
        character.gameObject.SetActive(characterData.IsActiveSelf);
        character.gameObject.transform.DOMove(characterData.Position, updateSec).SetEase(Ease.Linear);
        character.gameObject.transform.localScale = characterData.Scale;
        character.gameObject.transform.DORotateQuaternion(characterData.Rotation, updateSec).SetEase(Ease.Linear);
        character.SetAnimId(characterData.AnimationId);
        character.gameObject.GetComponent<DebuffController>().ApplyStatusEffect(false, characterData.DebuffList.ToArray());

        // �}�X�^�[�N���C�A���g�̏ꍇ�A�G��������悤�ɂ���
        if (RoomModel.Instance.IsMaster && character.tag == "Enemy" && !character.enabled)
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            character.enabled = true;
        }
        else
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            character.enabled = false;
        }
    }

    /// <summary>
    /// �v���C���[���擾
    /// </summary>
    /// <returns></returns>
    PlayerData GetPlayerData()
    {
        if (!playerObjs.ContainsKey(RoomModel.Instance.ConnectionId)) return null;
        var player = playerObjs[RoomModel.Instance.ConnectionId].GetComponent<PlayerBase>();
        var statusEffectController = player.GetComponent<DebuffController>();
        return new PlayerData()
        {
            IsActiveSelf = player.gameObject.activeInHierarchy,
            Status = new CharacterStatusData(
                hp: player.MaxHP,
                defence: player.MaxDefence,
                power: player.MaxPower,
                moveSpeed: player.MaxMoveSpeed,
                attackSpeedFactor: player.MaxAttackSpeedFactor,
                jumpPower: player.MaxJumpPower
                ),
            State = new CharacterStatusData(
                hp: player.HP,
                defence: player.defense,
                power: player.power,
                moveSpeed: player.moveSpeed,
                attackSpeedFactor: player.attackSpeedFactor,
                jumpPower: player.jumpPower
                ),
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
        List<EnemyData> enemyDatas = new List<EnemyData>();
        foreach (var key in enemies.Keys)
        {
            var enemyData = enemies[key];
            var enemy = enemyData.Enemy;
            var data = enemy.GetEnemyData();
            enemyDatas.Add(data);
        }
        return enemyDatas;
    }

    #region ���������֘A

    /// <summary>
    /// �ގ��ʒm
    /// </summary>
    /// <param name="joinedUser"></param>
    void OnLeave(JoinedUser joinedUser)
    {
        if (playerObjs.ContainsKey(joinedUser.ConnectionId))
        {
            var player = playerObjs[joinedUser.ConnectionId];
            playerObjs.Remove(joinedUser.ConnectionId);
            Destroy(player);

            // �G�������Ă���v���C���[�̃��X�g���X�V
            foreach(var enemy in enemies.Values)
            {
                enemy.Enemy.Players = playerObjs.Values.ToList();
            }
        }
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
    void OnUpdatePlayer(PlayerData playerData)
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
        foreach (var enemyData in masterClientData.EnemyDatas)
        {
            if (!enemies.ContainsKey(enemyData.EnemyID)
                || enemies.ContainsKey(enemyData.EnemyID) && enemies[enemyData.EnemyID].Enemy.HP <= 0) continue;

            var enemy = enemies[enemyData.EnemyID].Enemy;
            UpdateCharacter(enemyData, enemy);
            enemy.UpdateEnemy(enemyData);
        }
    }

    /// <summary>
    /// �G�̔�_���ʒm����
    /// </summary>
    void OnHitEnemy(EnemyDamegeData damageData)
    {
        GameObject? attacker = playerObjs.GetValueOrDefault(damageData.AttackerId);
        enemies[damageData.HitEnemyId].Enemy.ApplyDamage(damageData.Damage, damageData.RemainingHp, 
            playerObjs[damageData.AttackerId], true, damageData.DebuffList.ToArray());
    }
    #endregion
}
