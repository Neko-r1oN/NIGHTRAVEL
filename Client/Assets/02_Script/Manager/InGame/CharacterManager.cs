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
using UnityEngine.TextCore.Text;
using static UnityEditor.Experimental.GraphView.GraphView;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System.Xml;
using System.Threading.Tasks;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;

public class CharacterManager : MonoBehaviour
{
    #region �v���C���[�֘A
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // �e�v���C���[�̏����ʒu
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject charaGunnerPrefab;
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
    Dictionary<string, SpawnedEnemy> enemies = new Dictionary<string, SpawnedEnemy>();

    /// <summary>
    /// ���݂̃X�e�[�W�Ő��������G�̃��X�g
    /// </summary>
    public Dictionary<string, SpawnedEnemy> Enemies { get { return enemies; } }
    #endregion

    #region ���˕��֘A
    [SerializeField]
    List<GameObject> projectilePrefabs = new List<GameObject>();

    Dictionary<PROJECTILE_TYPE, GameObject> projectilePrefabsByType = new Dictionary<PROJECTILE_TYPE, GameObject>();
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
        RoomModel.Instance.OnEnemyHealthSyn += this.OnHitEnemy;
        RoomModel.Instance.OnChangedMasterClient += this.ActivateAllEnemies;
        RoomModel.Instance.OnShootedBullet += this.OnShootedBullet;
        RoomModel.Instance.OnUpdateStatusSyn += OnUpdatePlayerStatus;
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
        RoomModel.Instance.OnEnemyHealthSyn -= this.OnHitEnemy;
        RoomModel.Instance.OnChangedMasterClient += this.ActivateAllEnemies;
        RoomModel.Instance.OnShootedBullet -= this.OnShootedBullet;
        RoomModel.Instance.OnUpdateStatusSyn -= OnUpdatePlayerStatus;
    }


    /// <summary>
    /// �L�����N�^�[�̏��X�V�Ăяo���p�R���[�`��
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (GameManager.Instance.IsGameStart)
            {
                if (RoomModel.Instance.IsMaster) UpdateMasterDataRequest();
                else UpdatePlayerDataRequest();
            }
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
        foreach (var joinduser in RoomModel.Instance.joinedUserList)
        {
            var point = startPoints[0];
            startPoints.RemoveAt(0);

            if(joinduser.Value.CharacterID == 1)
            {
                var playerObj = Instantiate(charaSwordPrefab, point.position, Quaternion.identity);

                playerObjs.Add(joinduser.Key, playerObj);

                if (joinduser.Key == RoomModel.Instance.ConnectionId)
                {
                    playerObjSelf = playerObj;
                    Camera.main.gameObject.GetComponent<CameraFollow>().Target = playerObjSelf.transform;
                }
            }
            else if(joinduser.Value.CharacterID == 2)
            {
                var playerObj = Instantiate(charaGunnerPrefab, point.position, Quaternion.identity);

                playerObjs.Add(joinduser.Key, playerObj);

                if (joinduser.Key == RoomModel.Instance.ConnectionId)
                {
                    playerObjSelf = playerObj;
                    Camera.main.gameObject.GetComponent<CameraFollow>().Target = playerObjSelf.transform;
                }
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
            if(!enemies.ContainsKey(enemy.UniqueId)) enemies.Add(enemy.UniqueId, enemy);
        }
    }

    /// <summary>
    /// ���X�g����G���폜
    /// </summary>
    public void RemoveEnemyFromList(string uniqueId)
    {
        if (enemies.ContainsKey(uniqueId)) enemies.Remove(uniqueId);
    }

    /// <summary>
    /// SPAWN_ENEMY_TYPE�̒l�ɊY������G�����Ԃ�
    /// </summary>
    /// <param name="spawnType"></param>
    /// <returns></returns>
    public List<GameObject> GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE spawnType)
    {
        List<GameObject> result = new List<GameObject>();   // �Ԃ��G�̃��X�g
        foreach (var data in enemies)
        {
            if (data.Value.SpawnType == spawnType && data.Value.Enemy.HP > 0) result.Add(data.Value.Object);
        }

        return result;
    }

    /// <summary>
    /// ���˕��̃v���t�@�u���^�C�v���ɂ܂Ƃ߂�
    /// </summary>
    public void SetProjectilePrefabsByType()
    {
        foreach(var prefab in projectilePrefabs)
        {
            projectilePrefabsByType.Add(prefab.GetComponent<ProjectileBase>().TypeId, prefab);
        }
    }

    /// <summary>
    /// �L�����N�^�[�̏����X�V����
    /// </summary>
    /// <param name="characterData"></param>
    /// <param name="character"></param>
    void UpdateCharacter(CharacterData characterData, CharacterBase character)
    {
        var statusData = characterData.Status;
        var stateData = characterData.State;

        List<STATUS_TYPE> addStatusTypes = new List<STATUS_TYPE>() { STATUS_TYPE.All };
        if (character.gameObject.tag == "Enemy")
        {
            // �G�̏ꍇ��HP�ȊO���X�V����
            addStatusTypes = new List<STATUS_TYPE>() {
                STATUS_TYPE.Defense,
                STATUS_TYPE.Power,
                STATUS_TYPE.JumpPower,
                STATUS_TYPE.MoveSpeed,
                STATUS_TYPE.AttackSpeedFactor
            };
        }
        character.OverridMaxStatus(statusData, addStatusTypes.ToArray());
        character.OverridCurrentStatus(stateData, addStatusTypes.ToArray());
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
            foreach (var enemy in enemies.Values)
            {
                if (enemy.Enemy.Target == player) enemy.Enemy.GetNearPlayer(enemy.Enemy.transform.position);
            }
        }
    }

    /// <summary>
    /// �}�X�^�̌��������n���ꂽ�Ƃ��ɁA�S�Ă̓G�̃X�N���v�g���A�N�e�B�u�ɂ���
    /// </summary>
    void ActivateAllEnemies()
    {
        foreach (var enemy in enemies.Values)
        {
            enemy.Enemy.enabled = true;
        }
    }

    #region ���N�G�X�g�֘A

    /// <summary>
    /// �}�X�^�[�N���C�A���g�p�̏��X�V
    /// </summary>
    async void UpdateMasterDataRequest()
    {
        TimerDirector.Instance.Second -= updateSec;

        var masterClientData = new MasterClientData()
        {
            PlayerData = GetPlayerData(),
            EnemyDatas = GetEnemyDatas(),
            GimmickDatas = GimmickManager.Instance.GetGimmickDatas(),
            GameTimer = TimerDirector.Instance.Second
        };

        // �Q�[���^�C�}�[�X�V
        TimerDirector.Instance.OnUpdateTimer(masterClientData.GameTimer);

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
    #endregion

    #region �ʒm�����֘A

    /// <summary>
    /// �v���C���[�̃X�e�[�^�X�X�V�ʒm
    /// </summary>
    void OnUpdatePlayerStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        playerObjSelf.GetComponent<CharacterBase>().OverridMaxStatus(characterStatus);
        playerObjSelf.GetComponent<PlayerBase>().ChangeRelicStatusData(prsData);
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
        if (player == null) Debug.Log("�f�[�^�������Ă��܂���");
        UpdateCharacter(masterClientData.PlayerData, player);

        // �G�̏��X�V
        foreach (var enemyData in masterClientData.EnemyDatas)
        {
            if (!enemies.ContainsKey(enemyData.UniqueId)
                || enemies.ContainsKey(enemyData.UniqueId) && enemies[enemyData.UniqueId].Enemy.HP <= 0) continue;

            var enemy = enemies[enemyData.UniqueId].Enemy;
            UpdateCharacter(enemyData, enemy);
            enemy.UpdateEnemy(enemyData);
        }

        // �M�~�b�N�̏��X�V
        GimmickManager.Instance.UpdateGimmicks(masterClientData.GimmickDatas);

        // �Q�[���^�C�}�[�X�V
        TimerDirector.Instance.OnUpdateTimer(masterClientData.GameTimer);
    }

    /// <summary>
    /// �G�̔�_���ʒm����
    /// </summary>
    void OnHitEnemy(EnemyDamegeData damageData)
    {
        GameObject? attacker = playerObjs.GetValueOrDefault(damageData.AttackerId);
        enemies[damageData.HitEnemyId].Enemy.ApplyDamage(damageData.Damage, damageData.RemainingHp,
            playerObjs[damageData.AttackerId], true, true, damageData.DebuffList.ToArray());
    }

    /// <summary>
    /// ���˕��̐����ʒm
    /// </summary>
    /// <param name="type"></param>
    /// <param name="spawnPos"></param>
    /// <param name="shootVec"></param>
    void OnShootedBullet(PROJECTILE_TYPE type, List<DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
    {
        var projectile =  Instantiate(projectilePrefabsByType[type], spawnPos, rotation);
        projectile.GetComponent<ProjectileBase>().Init(debuffs, power);
        projectile.GetComponent<ProjectileBase>().Shoot(shootVec);
    }
    #endregion

    #endregion
}
