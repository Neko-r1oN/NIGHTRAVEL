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
    #region プレイヤー関連
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // 各プレイヤーの初期位置
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject charaGunnerPrefab;
    [SerializeField] GameObject playerObjSelf;  // ローカル用に属性付与
    Dictionary<Guid, GameObject> playerObjs = new Dictionary<Guid, GameObject>();

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
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }

        

        if (!RoomModel.Instance || RoomModel.Instance.ConnectionId == Guid.Empty)
        {
            if (!playerObjSelf)
            {
                Debug.LogError("playerObjSelfが設定されていない");
            }
            playerObjs.Add(Guid.Empty, playerObjSelf);

            return;
        }

        

        DestroyExistingPlayer();
        GenerateCharacters();

        // 通知処理を登録
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

        // シーン遷移したときに登録した通知処理を解除
        RoomModel.Instance.OnUpdatePlayerSyn -= this.OnUpdatePlayer;
        RoomModel.Instance.OnUpdateMasterClientSyn -= this.OnUpdateMasterClient;
        RoomModel.Instance.OnLeavedUser -= this.OnLeave;
        RoomModel.Instance.OnEnemyHealthSyn -= this.OnHitEnemy;
        RoomModel.Instance.OnChangedMasterClient += this.ActivateAllEnemies;
        RoomModel.Instance.OnShootedBullet -= this.OnShootedBullet;
        RoomModel.Instance.OnUpdateStatusSyn -= OnUpdatePlayerStatus;
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
    /// 新たな敵をリストに追加する
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
    /// 発射物のプレファブをタイプ事にまとめる
    /// </summary>
    public void SetProjectilePrefabsByType()
    {
        foreach(var prefab in projectilePrefabs)
        {
            projectilePrefabsByType.Add(prefab.GetComponent<ProjectileBase>().TypeId, prefab);
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
        character.SetAnimId(characterData.AnimationId);
        character.gameObject.GetComponent<DebuffController>().ApplyStatusEffect(false, characterData.DebuffList.ToArray());

        // マスタークライアントの場合、敵が動けるようにする
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

            // 以下は専用変数
            PlayerID = 0,   // ######################################################### とりあえず0固定
            ConnectionId = RoomModel.Instance.ConnectionId,
            IsDead = false
        };
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
            var enemyData = enemies[key];
            var enemy = enemyData.Enemy;
            var data = enemy.GetEnemyData();
            enemyDatas.Add(data);
        }
        return enemyDatas;
    }

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
            GameTimer = TimerDirector.Instance.Second
        };

        // ゲームタイマー更新
        TimerDirector.Instance.OnUpdateTimer(masterClientData.GameTimer);

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
    /// プレイヤーのステータス更新通知
    /// </summary>
    void OnUpdatePlayerStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        playerObjSelf.GetComponent<CharacterBase>().OverridMaxStatus(characterStatus);
        playerObjSelf.GetComponent<PlayerBase>().ChangeRelicStatusData(prsData);
    }

    /// <summary>
    /// プレイヤーの更新の通知
    /// </summary>
    /// <param name="playerData"></param>
    void OnUpdatePlayer(PlayerData playerData)
    {
        if (!playerObjs.ContainsKey(playerData.ConnectionId)) return;

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
        if (!playerObjs.ContainsKey(masterClientData.PlayerData.ConnectionId)) return;

        // プレイヤーの情報更新
        var player = playerObjs[masterClientData.PlayerData.ConnectionId].GetComponent<PlayerBase>();
        if (player == null) Debug.Log("データが入っていません");
        UpdateCharacter(masterClientData.PlayerData, player);

        // 敵の情報更新
        foreach (var enemyData in masterClientData.EnemyDatas)
        {
            if (!enemies.ContainsKey(enemyData.UniqueId)
                || enemies.ContainsKey(enemyData.UniqueId) && enemies[enemyData.UniqueId].Enemy.HP <= 0) continue;

            var enemy = enemies[enemyData.UniqueId].Enemy;
            UpdateCharacter(enemyData, enemy);
            enemy.UpdateEnemy(enemyData);
        }

        // ギミックの情報更新
        GimmickManager.Instance.UpdateGimmicks(masterClientData.GimmickDatas);

        // ゲームタイマー更新
        TimerDirector.Instance.OnUpdateTimer(masterClientData.GameTimer);
    }

    /// <summary>
    /// 敵の被ダメ通知処理
    /// </summary>
    void OnHitEnemy(EnemyDamegeData damageData)
    {
        GameObject? attacker = playerObjs.GetValueOrDefault(damageData.AttackerId);
        enemies[damageData.HitEnemyId].Enemy.ApplyDamage(damageData.Damage, damageData.RemainingHp,
            playerObjs[damageData.AttackerId], true, true, damageData.DebuffList.ToArray());
    }

    /// <summary>
    /// 発射物の生成通知
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
