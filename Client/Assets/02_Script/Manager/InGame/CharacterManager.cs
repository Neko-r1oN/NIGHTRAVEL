//**************************************************
//  存在しているキャラクターの管理を行う
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
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    #region プレイヤー関連
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // 各プレイヤーの初期位置
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject charaGunnerPrefab;
    [SerializeField] GameObject offScreenUIPrefab;

    [SerializeField] GameObject playerObjSelf;  // ローカル用に属性付与
    Dictionary<Guid, GameObject> playerObjs = new Dictionary<Guid, GameObject>();

    Dictionary<Guid,GameObject> playerUIObjs = new Dictionary<Guid, GameObject>();
    public Dictionary<Guid, GameObject> PlayerUIObjs {  get { return playerUIObjs; } }

    /// <summary>
    /// 自身のキャラクターデータ(シーン遷移したときの引継ぎ用)
    /// </summary>
    static public PlayerStatusData SelfPlayerStatusData { get; set; } = null;

    /// <summary>
    /// 自分の操作キャラ
    /// </summary>
    public GameObject PlayerObjSelf { get { return playerObjSelf; } }

    /// <summary>
    /// プレイヤーのリスト
    /// </summary>
    public Dictionary<Guid, GameObject> PlayerObjs { get { return playerObjs; } }
    #endregion

    #region 敵関連
    Dictionary<string, SpawnedEnemy> enemies = new Dictionary<string, SpawnedEnemy>();

    /// <summary>
    /// 現在のステージで生成した敵のリスト
    /// </summary>
    public Dictionary<string, SpawnedEnemy> Enemies { get { return enemies; } }
    #endregion

    #region 発射物関連
    [SerializeField]
    List<GameObject> projectilePrefabs = new List<GameObject>();

    Dictionary<PROJECTILE_TYPE, GameObject> projectilePrefabsByType = new Dictionary<PROJECTILE_TYPE, GameObject>();
    #endregion

    #region カメラ関連
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
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }

        // オフライン用
        if (!RoomModel.Instance || RoomModel.Instance.ConnectionId == Guid.Empty)
        {
            if (!playerObjSelf)
            {
                Debug.LogError("playerObjSelfが設定されていない");
            }

            // ステージ上の全てのプレイヤーオブジェクトをリストに登録
            playerObjs.Add(Guid.Empty, playerObjSelf);
            foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (playerObjSelf != player)
                {
                    playerObjs.Add(Guid.NewGuid(), player);
                }
            }

            // プレイヤーのステータス引継ぎ設定
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

        // 既にステージに配置されているプレイヤーを削除し、参加人数分プレイヤー生成
        DestroyExistingPlayer();
        GenerateCharacters();

        // プレイヤーのステータス引継ぎ設定
        if (SelfPlayerStatusData == null) UpdateSelfSelfPlayerStatusData();
        else ApplySelfPlayerStatusData();

        // 通知処理を登録
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

        // シーン遷移したときに登録した通知処理を解除
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
    }

    #region キャラクター関連

    /// <summary>
    /// マネージャーで保持しているプレイヤーのステータスデータを更新する
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
    /// マネージャーで保持しているプレイヤーのステータスデータを適用させる
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
    /// キャラクターの情報更新呼び出し用コルーチン
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
    /// キャラクターの情報を更新する
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
            // 敵の場合はHP以外を更新する
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

        // 操作キャラ以外のプレイヤーオブジェクトが震えないようにする
        if (character.tag == "Player" && character.gameObject != playerObjSelf)
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            character.enabled = false;
        }
    }

    #region プレイヤー関連

    /// <summary>
    /// 生存しているプレイヤーをリストにまとめて返す
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetAlivePlayers()
    {
        List<GameObject> result = new List<GameObject>();
        foreach(var player in PlayerObjs)
        {
            if(IsPlayerAlive(player.Key)) result.Add(player.Value);
        }
        return result;
    }

    /// <summary>
    /// 指定した操作キャラの生存確認
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsPlayerAlive(Guid id)
    {
        return playerObjs.ContainsKey(id) && playerObjs[id] && !playerObjs[id].GetComponent<PlayerBase>().IsDead;
    }

    /// <summary>
    /// 既にシーン上に存在しているプレイヤーを破棄する
    /// </summary>
    void DestroyExistingPlayer()
    {
        // 既にシーン上に存在している操作キャラを削除する
        var players = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }
    }

    /// <summary>
    /// 参加しているユーザー情報を元に、プレイヤーを生成する
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

                var target = new CameraTarget();
                target.TrackingTarget = playerObjSelf.transform;
                target.LookAtTarget = playerObjSelf.transform;
                camera.GetComponent<CinemachineCamera>().Target.TrackingTarget = playerObjSelf.transform;
            }

            playerObj.transform.Find("Camera").GetComponent<Camera>().targetTexture = playerUIList[count];

            // 画面外UIの作成
            var obj = GameObject.Find("OffScreenUI").transform;
            var playerUI = Instantiate(offScreenUIPrefab, Vector3.zero, Quaternion.identity, obj);
            playerUI.gameObject.name = "player" + count + 1; 
            playerUI.transform.Find("Arow/Image/RawImage").GetComponent<RawImage>().texture 
                = playerUIList[count];
            playerUI.GetComponent<PlayerUI>().target = playerObj.transform;

            playerUI.GetComponentInChildren<Text>().text
                = joinduser.Value.UserData.Name;

            playerUIObjs.Add(joinduser.Value.ConnectionId, playerUI);

            count++;
        }
    }

    /// <summary>
    /// 自身以外のプレイヤーをListにして返す
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
    /// プレイヤー情報取得
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

            // 以下は専用変数
            ConnectionId = RoomModel.Instance.ConnectionId,
            IsDead = player.IsDead
        };
    }

    /// <summary>
    /// 通知があったキャラのビームエフェクトのON/OFF
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

    #region 敵関連

    /// <summary>
    /// 難易度を基に全ての敵のステータスを上昇させる
    /// </summary>
    public void ApplyDifficultyToAllEnemies()
    {
        foreach(var enemy in Enemies)
        {
            enemy.Value.Enemy.ApplyDifficultyBasedStatusBoost();
        }
    }

    /// <summary>
    /// 新たな敵をリストに追加する
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
    /// リストから敵を削除
    /// </summary>
    public void RemoveEnemyFromList(string uniqueId)
    {
        if (enemies.ContainsKey(uniqueId)) enemies.Remove(uniqueId);
    }

    /// <summary>
    /// SPAWN_ENEMY_TYPEの値に該当する敵だけ返す
    /// </summary>
    /// <param name="spawnType"></param>
    /// <returns></returns>
    public List<GameObject> GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE spawnType)
    {
        List<GameObject> result = new List<GameObject>();   // 返す敵のリスト
        foreach (var data in enemies)
        {
            if (data.Value.SpawnType == spawnType && data.Value.Enemy.HP > 0) result.Add(data.Value.Object);
        }

        return result;
    }

    /// <summary>
    /// 指定した端末IDに紐づく敵を返す
    /// </summary>
    /// <param name="terminalID"></param>
    /// <returns></returns>
    public List<GameObject> GetEnemysByTerminalID(int terminalID)
    {
        List<GameObject> result = new List<GameObject>();   // 返す敵のリスト
        foreach (var data in enemies)
        {
            if (data.Value.TerminalID == terminalID && data.Value.Enemy.HP > 0) result.Add(data.Value.Object);
        }
        return result;
    }

    /// <summary>
    /// 指定端末に紐づく敵の削除
    /// </summary>
    /// <param name="termID"></param>
    public void DeleteTerminalEnemy(int termID)
    {
        // 削除対象のキーを一時的に保存するリスト
        var removeKeys = new List<string>();

        foreach (var data in enemies)
        {
            if (data.Value.TerminalID == termID && data.Value.Enemy.HP > 0)
            {
                Destroy(data.Value.Object); // ゲームオブジェクトを削除
                removeKeys.Add(data.Key);   // 辞書から削除するキーを追加
            }
        }

        // ループが終わってからまとめて削除
        foreach (var key in removeKeys)
        {
            enemies.Remove(key);
        }
    }

    /// <summary>
    /// 敵の情報取得
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
    /// ボスを返す処理
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

    #region 発射物関連

    /// <summary>
    /// 発射物のプレファブをタイプ事にまとめる
    /// </summary>
    public void SetProjectilePrefabsByType()
    {
        foreach (var prefab in projectilePrefabs)
        {
            projectilePrefabsByType.Add(prefab.GetComponent<ProjectileBase>().TypeId, prefab);
        }
    }

    #endregion

    #region 同期処理関連

    /// <summary>
    /// 退室通知
    /// </summary>
    /// <param name="joinedUser"></param>
    void OnLeave(JoinedUser joinedUser)
    {
        if (playerObjs.ContainsKey(joinedUser.ConnectionId))
        {
            var player = playerObjs[joinedUser.ConnectionId];
            playerObjs.Remove(joinedUser.ConnectionId);
            Destroy(player);

            // 敵が持っているプレイヤーのリストを更新
            foreach (var enemy in enemies.Values)
            {
                if (enemy.Enemy.Target == player) enemy.Enemy.GetNearPlayer(enemy.Enemy.transform.position);
            }
        }
    }

    /// <summary>
    /// マスタの権限が譲渡されたときに、全ての敵のスクリプトをアクティブにする
    /// </summary>
    void ActivateAllEnemies()
    {
        foreach (var enemy in enemies.Values)
        {
            enemy.Enemy.enabled = true;
        }
    }

    #region リクエスト関連

    /// <summary>
    /// マスタークライアント用の情報更新
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

        // マスタクライアント情報更新リクエスト
        await RoomModel.Instance.UpdateMasterClientAsync(masterClientData);
    }

    /// <summary>
    /// プレイヤーの情報更新
    /// </summary>
    async void UpdatePlayerDataRequest()
    {
        var playerData = GetPlayerData();

        // プレイヤー情報更新リクエスト
        await RoomModel.Instance.UpdatePlayerAsync(playerData);
    }
    #endregion

    #region 通知処理関連

    /// <summary>
    /// プレイヤー死亡同期
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnPlayerDead(Guid uniqueId)
    {
        playerObjs[uniqueId].GetComponent<PlayerBase>().OnDead();

        Destroy(playerUIObjs[uniqueId]);
    }

    /// <summary>
    /// プレイヤーのステータス更新通知
    /// </summary>
    void OnUpdatePlayerStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        if (playerObjSelf)
        {
            playerObjSelf.GetComponent<CharacterBase>().OverridMaxStatus(characterStatus,STATUS_TYPE.All);
            playerObjSelf.GetComponent<PlayerBase>().ChangeRelicStatusData(prsData);
            UIManager.Instance.UpdatePlayerStatus();
        }
    }

    /// <summary>
    /// プレイヤーの更新の通知
    /// </summary>
    /// <param name="playerData"></param>
    void OnUpdatePlayer(PlayerData playerData)
    {
        if (!playerObjs.ContainsKey(playerData.ConnectionId) || !RoomModel.Instance) return;

        // プレイヤーの情報更新
        var player = playerObjs[playerData.ConnectionId].GetComponent<PlayerBase>();
        UpdateCharacter(playerData, player);
    }

    /// <summary>
    /// マスタークライアントの更新通知
    /// </summary>
    /// <param name="masterClientData"></param>
    void OnUpdateMasterClient(MasterClientData masterClientData)
    {
        if (RoomModel.Instance.IsMaster) return;
        if (!playerObjs.ContainsKey(masterClientData.PlayerData.ConnectionId)) return;

        // プレイヤーの情報更新
        var player = playerObjs[masterClientData.PlayerData.ConnectionId].GetComponent<PlayerBase>();
        if (player == null) Debug.Log("データが入っていません");
        UpdateCharacter(masterClientData.PlayerData, player);

        // 敵の情報更新
        foreach (var enemyData in masterClientData.EnemyDatas)
        {
            bool isEnemy = enemies.ContainsKey(enemyData.UniqueId);
            if (!isEnemy || isEnemy && enemies[enemyData.UniqueId].Enemy == null) continue;

            var enemy = enemies[enemyData.UniqueId].Enemy;
            UpdateCharacter(enemyData, enemy);
            enemy.UpdateEnemy(enemyData);
        }

        // ギミックの情報更新
        GimmickManager.Instance.UpdateGimmicks(masterClientData.GimmickDatas);

        // 端末情報の更新
        TerminalManager.Instance.OnUpdateTerminal(masterClientData.TerminalDatas);
    }

    /// <summary>
    /// 敵の被ダメ通知処理
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
                // ザコ敵のときだけノックバックさせる
                isKnockback = !enemy.IsBoss && enemy.EnemyTypeId != ENEMY_TYPE.MetalBody;
                attacker = playerObjs[damageData.AttackerId];
            }
            enemy.ApplyDamage(damageData.Damage, damageData.RemainingHp, attacker, isKnockback, true, damageData.DebuffList.ToArray());

            if (isAttackerAlive && RoomModel.Instance.ConnectionId == damageData.AttackerId)
            {   // レリック「リゲインコード」所有時、与ダメージの一部をHP回復
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
    /// 弾の発射通知
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
    /// レベルアップ通知
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

    #endregion

    #endregion
}
