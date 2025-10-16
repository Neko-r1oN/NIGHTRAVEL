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
using DG.Tweening;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using UnityEngine.TextCore.Text;
using Unity.VisualScripting.FullSerializer;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Xml;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    #region �v���C���[�֘A
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // �e�v���C���[�̏����ʒu
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject charaGunnerPrefab;
    [SerializeField] GameObject offScreenUIPrefab;

    [SerializeField] GameObject playerObjSelf;  // ���[�J���p�ɑ����t�^
    Dictionary<Guid, GameObject> playerObjs = new Dictionary<Guid, GameObject>();

    Dictionary<Guid,GameObject> playerUIObjs = new Dictionary<Guid, GameObject>();
    public Dictionary<Guid, GameObject> PlayerUIObjs {  get { return playerUIObjs; } }

    /// <summary>
    /// ���g�̃L�����N�^�[�f�[�^(�V�[���J�ڂ����Ƃ��̈��p���p)
    /// </summary>
    static public PlayerStatusData SelfPlayerStatusData { get; set; } = null;

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

    #region �J�����֘A
    [SerializeField] GameObject camera;
    [SerializeField] CinemachineTargetGroup cinemachineTargetGroup;

    [SerializeField] RenderTexture[] playerUIList;
    #endregion

    const float updateSec = 0.1f;

    public float UpdateSec { get { return updateSec; } }

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

        // �I�t���C���p
        if (!RoomModel.Instance || RoomModel.Instance.ConnectionId == Guid.Empty)
        {
            if (!playerObjSelf)
            {
                Debug.LogError("playerObjSelf���ݒ肳��Ă��Ȃ�");
            }
            playerObjs.Add(Guid.Empty, playerObjSelf);

            // �v���C���[�̃X�e�[�^�X���p���ݒ�
            if (SelfPlayerStatusData == null) UpdateSelfSelfPlayerStatusData();
            else ApplySelfPlayerStatusData();

            if (cinemachineTargetGroup)
            {
                var newTarget = new CinemachineTargetGroup.Target
                {
                    Object = playerObjSelf.transform,
                    Radius = 1f,
                    Weight = 1f
                };
                cinemachineTargetGroup.Targets.Add(newTarget);
            }

            return;
        }

        // ���ɃX�e�[�W�ɔz�u����Ă���v���C���[���폜���A�Q���l�����v���C���[����
        DestroyExistingPlayer();
        GenerateCharacters();

        // �v���C���[�̃X�e�[�^�X���p���ݒ�
        if (SelfPlayerStatusData == null) UpdateSelfSelfPlayerStatusData();
        else ApplySelfPlayerStatusData();

        // �ʒm������o�^
        RoomModel.Instance.OnUpdatePlayerSyn += this.OnUpdatePlayer;
        RoomModel.Instance.OnUpdateMasterClientSyn += this.OnUpdateMasterClient;
        RoomModel.Instance.OnLeavedUser += this.OnLeave;
        RoomModel.Instance.OnEnemyHealthSyn += this.OnHitEnemy;
        RoomModel.Instance.OnChangedMasterClient += this.ActivateAllEnemies;
        RoomModel.Instance.OnShootedBullet += this.OnShootedBullet;
        RoomModel.Instance.OnUpdateStatusSyn += this.OnUpdatePlayerStatus;
        RoomModel.Instance.OnLevelUpSyn += this.OnLevelup;
        RoomModel.Instance.OnPlayerDeadSyn += this.OnPlayerDead;
        RoomModel.Instance.OnBeamEffectActived += this.OnBeamEffectActived;
        RoomModel.Instance.OnDeleteEnemySyn += this.OnDeleteEnemy;
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        StartCoroutine("UpdateCoroutine");
        SetProjectilePrefabsByType();
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
        RoomModel.Instance.OnChangedMasterClient -= this.ActivateAllEnemies;
        RoomModel.Instance.OnShootedBullet -= this.OnShootedBullet;
        RoomModel.Instance.OnUpdateStatusSyn -= this.OnUpdatePlayerStatus;
        RoomModel.Instance.OnLevelUpSyn -= this.OnLevelup;
        RoomModel.Instance.OnPlayerDeadSyn -= this.OnPlayerDead;
        RoomModel.Instance.OnBeamEffectActived -= this.OnBeamEffectActived;
        RoomModel.Instance.OnDeleteEnemySyn -= this.OnDeleteEnemy;
    }

    #region �L�����N�^�[�֘A

    /// <summary>
    /// �}�l�[�W���[�ŕێ����Ă���v���C���[�̃X�e�[�^�X�f�[�^���X�V����
    /// </summary>
    public void UpdateSelfSelfPlayerStatusData()
    {
        SelfPlayerStatusData = new PlayerStatusData()
        {
            NowLevel = playerObjSelf.GetComponent<PlayerBase>().NowLv,
            NowExp = playerObjSelf.GetComponent<PlayerBase>().NowExp,
            NextLevelExp = playerObjSelf.GetComponent<PlayerBase>().NextLvExp,
            CharacterMaxStatusData = playerObjSelf.GetComponent<CharacterBase>().GetCurrentMaxStatusData(),
            PlayerRelicStatusData = playerObjSelf.GetComponent<PlayerBase>().GetPlayerRelicStatusData(),
        };
    }

    /// <summary>
    /// �}�l�[�W���[�ŕێ����Ă���v���C���[�̃X�e�[�^�X�f�[�^��K�p������
    /// </summary>
    public void ApplySelfPlayerStatusData()
    {
        playerObjSelf.GetComponent<PlayerBase>().NowExp = SelfPlayerStatusData.NowExp;
        playerObjSelf.GetComponent<PlayerBase>().NowLv = SelfPlayerStatusData.NowLevel;
        playerObjSelf.GetComponent<PlayerBase>().NextLvExp = SelfPlayerStatusData.NextLevelExp;
        playerObjSelf.GetComponent<CharacterBase>().OverridMaxStatus(SelfPlayerStatusData.CharacterMaxStatusData, STATUS_TYPE.All);
        playerObjSelf.GetComponent<CharacterBase>().OverridCurrentStatus(SelfPlayerStatusData.CharacterMaxStatusData, STATUS_TYPE.All);
        playerObjSelf.GetComponent<PlayerBase>().ChangeRelicStatusData(SelfPlayerStatusData.PlayerRelicStatusData);
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
        character.gameObject.GetComponent<DebuffController>().ApplyStatusEffect(false, characterData.DebuffList.ToArray());

        if (character.tag != "Enemy" || 
            (character.tag == "Enemy" && !character.GetComponent<EnemyBase>().IsHitAnimIdFrom(characterData.AnimationId)))
        {
            character.SetAnimId(characterData.AnimationId);
        }


        if (character.tag == "Enemy" && !character.GetComponent<EnemyBase>().IsStartComp) character.GetComponent<EnemyBase>().LoadStart();

        // ����L�����ȊO�̃v���C���[�I�u�W�F�N�g���k���Ȃ��悤�ɂ���
        if (character.tag == "Player" && character.gameObject != playerObjSelf)
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            character.enabled = false;
        }
    }

    #region �v���C���[�֘A

    /// <summary>
    /// �w�肵������L�����̐����m�F
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsPlayerAlive(Guid id)
    {
        return playerObjs.ContainsKey(id) && playerObjs[id] && !playerObjs[id].GetComponent<PlayerBase>().IsDead;
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
        int count = 0;

        foreach (var joinduser in RoomModel.Instance.joinedUserList)
        {
            var point = startPoints[0];
            startPoints.RemoveAt(0);

            var prefab = joinduser.Value.CharacterID == 1 ? charaSwordPrefab : charaGunnerPrefab;
            var playerObj = Instantiate(prefab, point.position, Quaternion.identity);
            playerObjs.Add(joinduser.Key, playerObj);

            if (joinduser.Key == RoomModel.Instance.ConnectionId)
            {
                playerObjSelf = playerObj;

                var target = new CameraTarget();
                target.TrackingTarget = playerObjSelf.transform;
                target.LookAtTarget = playerObjSelf.transform;
                camera.GetComponent<CinemachineCamera>().Target.TrackingTarget = playerObjSelf.transform;
            }

            if (cinemachineTargetGroup)
            {
                var newTarget = new CinemachineTargetGroup.Target
                {
                    Object = playerObjSelf.transform,
                    Radius = 1f,
                    Weight = 1f
                };
                cinemachineTargetGroup.Targets.Add(newTarget);
            }

            playerObj.transform.Find("Camera").GetComponent<Camera>().targetTexture = playerUIList[count];

            // ��ʊOUI�̍쐬
            var obj = GameObject.Find("OffScreenUI").transform;
            var playerUI = Instantiate(offScreenUIPrefab, Vector3.zero, Quaternion.identity, obj);
            playerUI.transform.Find("��/Image/RawImage").GetComponent<RawImage>().texture 
                = playerUIList[count];
            playerUI.GetComponent<PlayerUI>().target = playerObj.transform;

            playerUIObjs.Add(joinduser.Value.ConnectionId, playerUI);

            count++;
        }
    }

    /// <summary>
    /// ���g�ȊO�̃v���C���[��List�ɂ��ĕԂ�
    /// </summary>
    /// <returns></returns>
    public List<PlayerBase> GetPlayersExceptSelf()
    {
        List<PlayerBase> result = new List<PlayerBase>();
        foreach (var player in playerObjs.Values)
        {
            if (player != playerObjSelf)
            {
                if (player == null) result.Add(null);
                else result.Add(player.GetComponent<PlayerBase>());
            }
        }
        return result;
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
                jumpPower: player.MaxJumpPower,
                healRate: player.MaxHealRate
                ),
            State = new CharacterStatusData(
                hp: player.HP,
                defence: player.defense,
                power: player.power,
                moveSpeed: player.moveSpeed,
                attackSpeedFactor: player.attackSpeedFactor,
                jumpPower: player.jumpPower,
                healRate: player.healRate
                ),
            Position = player.transform.position,
            Scale = player.transform.localScale,
            Rotation = player.transform.rotation,
            AnimationId = player.GetAnimId(),
            DebuffList = statusEffectController.GetAppliedStatusEffects(),

            // �ȉ��͐�p�ϐ�
            ConnectionId = RoomModel.Instance.ConnectionId,
            IsDead = player.IsDead
        };
    }

    /// <summary>
    /// �ʒm���������L�����̃r�[���G�t�F�N�g��ON/OFF
    /// </summary>
    /// <param name="conID"></param>
    /// <param name="isActive"></param>
    private void OnBeamEffectActived(Guid conID, bool isActive)
    {
        if (playerObjs.ContainsKey(conID))
        {
            var playerEffect = playerObjs[conID].GetComponent<PlayerEffect>();
            if (playerEffect) playerEffect.BeamEffectActive(isActive);
        }
    }

    #endregion

    #region �G�֘A

    /// <summary>
    /// ��Փx����ɑS�Ă̓G�̃X�e�[�^�X���㏸������
    /// </summary>
    public void ApplyDifficultyToAllEnemies()
    {
        foreach(var enemy in Enemies)
        {
            enemy.Value.Enemy.ApplyDifficultyBasedStatusBoost();
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
            if (!enemies.ContainsKey(enemy.UniqueId)) enemies.Add(enemy.UniqueId, enemy);
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
    /// �w�肵���[��ID�ɕR�Â��G��Ԃ�
    /// </summary>
    /// <param name="terminalID"></param>
    /// <returns></returns>
    public List<GameObject> GetEnemysByTerminalID(int terminalID)
    {
        List<GameObject> result = new List<GameObject>();   // �Ԃ��G�̃��X�g
        foreach (var data in enemies)
        {
            if (data.Value.TerminalID == terminalID && data.Value.Enemy.HP > 0) result.Add(data.Value.Object);
        }
        return result;
    }

    /// <summary>
    /// �w��[���ɕR�Â��G�̍폜
    /// </summary>
    /// <param name="termID"></param>
    public void DeleteTerminalEnemy(int termID)
    {
        // �폜�Ώۂ̃L�[���ꎞ�I�ɕۑ����郊�X�g
        var removeKeys = new List<string>();

        foreach (var data in enemies)
        {
            if (data.Value.TerminalID == termID && data.Value.Enemy.HP > 0)
            {
                Destroy(data.Value.Object); // �Q�[���I�u�W�F�N�g���폜
                removeKeys.Add(data.Key);   // ��������폜����L�[��ǉ�
            }
        }

        // ���[�v���I����Ă���܂Ƃ߂č폜
        foreach (var key in removeKeys)
        {
            enemies.Remove(key);
        }
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
            if (enemies[key] == null || enemies[key].Enemy == null) continue;
            var enemyData = enemies[key];
            var enemy = enemyData.Enemy;
            var data = enemy.GetEnemyData();
            enemyDatas.Add(data);
        }
        return enemyDatas;
    }

    /// <summary>
    /// �{�X��Ԃ�����
    /// </summary>
    /// <returns></returns>
    public EnemyBase GetBossObject()
    {
        foreach (var enemy in enemies.Values)
        {
            if (enemy.Enemy.IsBoss)
            {
                return enemy.Enemy;
            }
        }

        return null;
    }

    #endregion

    #endregion

    #region ���˕��֘A

    /// <summary>
    /// ���˕��̃v���t�@�u���^�C�v���ɂ܂Ƃ߂�
    /// </summary>
    public void SetProjectilePrefabsByType()
    {
        foreach (var prefab in projectilePrefabs)
        {
            projectilePrefabsByType.Add(prefab.GetComponent<ProjectileBase>().TypeId, prefab);
        }
    }

    #endregion

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
            GameTimer = TimerDirector.Instance.Second,
            TerminalDatas = TerminalManager.Instance.TerminalDatas
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
    /// �v���C���[���S����
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnPlayerDead(Guid uniqueId)
    {
        playerObjs[uniqueId].GetComponent<PlayerBase>().OnDead();

        Destroy(playerUIObjs[uniqueId]);
    }

    /// <summary>
    /// �v���C���[�̃X�e�[�^�X�X�V�ʒm
    /// </summary>
    void OnUpdatePlayerStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        if (playerObjSelf)
        {
            playerObjSelf.GetComponent<CharacterBase>().OverridMaxStatus(characterStatus,STATUS_TYPE.All);
            playerObjSelf.GetComponent<PlayerBase>().ChangeRelicStatusData(prsData);
        }
    }

    /// <summary>
    /// �v���C���[�̍X�V�̒ʒm
    /// </summary>
    /// <param name="playerData"></param>
    void OnUpdatePlayer(PlayerData playerData)
    {
        if (!playerObjs.ContainsKey(playerData.ConnectionId) || !RoomModel.Instance) return;

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
        if (RoomModel.Instance.IsMaster) return;
        if (!playerObjs.ContainsKey(masterClientData.PlayerData.ConnectionId)) return;

        // �v���C���[�̏��X�V
        var player = playerObjs[masterClientData.PlayerData.ConnectionId].GetComponent<PlayerBase>();
        if (player == null) Debug.Log("�f�[�^�������Ă��܂���");
        UpdateCharacter(masterClientData.PlayerData, player);

        // �G�̏��X�V
        foreach (var enemyData in masterClientData.EnemyDatas)
        {
            bool isEnemy = enemies.ContainsKey(enemyData.UniqueId);
            if (!isEnemy || isEnemy && enemies[enemyData.UniqueId].Enemy == null) continue;

            var enemy = enemies[enemyData.UniqueId].Enemy;
            UpdateCharacter(enemyData, enemy);
            enemy.UpdateEnemy(enemyData);
        }

        // �M�~�b�N�̏��X�V
        GimmickManager.Instance.UpdateGimmicks(masterClientData.GimmickDatas);

        // �Q�[���^�C�}�[�X�V
        TimerDirector.Instance.OnUpdateTimer(masterClientData.GameTimer);

        // �[�����̍X�V
        TerminalManager.Instance.OnUpdateTerminal(masterClientData.TerminalDatas);
    }

    /// <summary>
    /// �G�̔�_���ʒm����
    /// </summary>
    void OnHitEnemy(EnemyDamegeData damageData)
    {
        if (enemies.ContainsKey(damageData.HitEnemyId))
        {
            var enemy = enemies[damageData.HitEnemyId].Enemy;
            GameObject attacker = null;
            bool isKnockback = false;
            bool isAttackerAlive = IsPlayerAlive(damageData.AttackerId);

            if (isAttackerAlive)
            {
                // �U�R�G�̂Ƃ������m�b�N�o�b�N������
                isKnockback = !enemy.IsBoss && enemy.EnemyTypeId != ENEMY_TYPE.MetalBody;
                attacker = playerObjs[damageData.AttackerId];
            }
            enemy.ApplyDamage(damageData.Damage, damageData.RemainingHp, attacker, isKnockback, true, damageData.DebuffList.ToArray());

            if (isAttackerAlive && RoomModel.Instance.ConnectionId == damageData.AttackerId)
            {   // �����b�N�u���Q�C���R�[�h�v���L���A�^�_���[�W�̈ꕔ��HP��
                var plBase = playerObjSelf.GetComponent<PlayerBase>();

                if (plBase.DmgHealRate > 0)
                {
                    plBase.HP += (int)(damageData.Damage * plBase.DmgHealRate);

                    if (plBase.HP > plBase.MaxHP) plBase.HP = plBase.MaxHP;
                }
            }

            if (damageData.Exp > 0)
            {
                playerObjSelf.GetComponent<PlayerBase>().NowExp += damageData.Exp;
                if (UIManager.Instance) UIManager.Instance.UpdateExperienceAndLevel();
            }
        }
    }

    /// <summary>
    /// �e�̔��˒ʒm
    /// </summary>
    /// <param name="type"></param>
    /// <param name="spawnPos"></param>
    /// <param name="shootVec"></param>
    void OnShootedBullet(List<ShootBulletData> shootBulletDatas)
    {
        foreach(var shootBulletData in shootBulletDatas)
        {
            var projectile = Instantiate(projectilePrefabsByType[shootBulletData.Type], shootBulletData.SpawnPos, shootBulletData.Rotation);
            projectile.GetComponent<ProjectileBase>().Init(shootBulletData.Debuffs, shootBulletData.Power);
            projectile.GetComponent<ProjectileBase>().Shoot(shootBulletData.ShootVec);
        }
    }

    /// <summary>
    /// ���x���A�b�v�ʒm
    /// </summary>
    /// <param name="level"></param>
    /// <param name="nowExp"></param>
    /// <param name="characterStatusDataList"></param>
    /// <param name="optionsKey"></param>
    /// <param name="statusOptionList"></param>
    void OnLevelup(int level, int nowExp, int nextExp, CharacterStatusData updatedStatusData, Guid optionsKey, List<StatusUpgrateOptionData> statusOptionList)
    {
        if (IsPlayerAlive(RoomModel.Instance.ConnectionId))
        {
            var player = playerObjSelf.GetComponent<PlayerBase>();
            player.NowExp = nowExp;
            player.NowLv = level;
            player.NextLvExp = nextExp;
            player.OverridMaxStatus(updatedStatusData, STATUS_TYPE.HP, STATUS_TYPE.Power, STATUS_TYPE.Defense);
            if (UIManager.Instance) UIManager.Instance.UpdateExperienceAndLevel();
        }
        LevelManager.Options.Add(optionsKey, statusOptionList);
    }

    /// <summary>
    /// �G�̍폜�ʒm
    /// </summary>
    /// <param name="enemId"></param>
    void OnDeleteEnemy(string enemId)
    {
        if(enemies.ContainsKey(enemId))
        {
            Destroy(enemies[enemId].Enemy.gameObject);
            RemoveEnemyFromList(enemId);
        }
    }

    #endregion

    #endregion
}
