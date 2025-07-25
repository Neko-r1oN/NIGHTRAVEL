//**************************************************
//  存在しているキャラクターの管理を行う
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
    #region プレイヤー関連
    [SerializeField] List<Transform> startPoints = new List<Transform>();   // 各プレイヤーの初期位置
    [SerializeField] GameObject charaSwordPrefab;
    [SerializeField] GameObject playerObjSelf;  // ローカル用に属性付与
    Dictionary<Guid, GameObject> playerObjs = new Dictionary<Guid, GameObject>();

    /// <summary>
    /// 自分の操作キャラ
    /// </summary>
    public GameObject PlayerObjSelf { get { return playerObjSelf; } }
    #endregion

    #region 敵関連
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
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
        if (RoomModel.Instance.ConnectionId == Guid.Empty) return;
        DestroyExistingPlayer();
        GenerateCharacters();
        StartCoroutine("UpdateCoroutine");
        isAwake = true;
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
            Destroy(player);
        }
    }

    /// <summary>
    /// 参加しているユーザー情報を元に、プレイヤーを生成する
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
    /// キャラクターの情報更新呼び出し用コルーチン
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        if (RoomModel.Instance.IsMaster) UpdateMasterData();
        else UpdatePlayerData();
        yield return new WaitForSeconds(updateSec);
    }

    /// <summary>
    /// プレイヤー情報取得
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

            // 以下は専用変数
            PlayerID = 0,   // ######################################################### とりあえず0固定
            IsDead = false
        };
    }

    /// <summary>
    /// 敵の情報取得
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

                // 以下は専用変数
                EnemyID = key,
                EnemyName = enemyObj.name,
                isBoss = enemy.IsBoss,
            };
        }
        return enemyDatas;
    }

    /// <summary>
    /// マスタークライアント用の情報更新
    /// </summary>
    async void UpdateMasterData()
    {
        var masterClientData = new MasterClientData()
        {
            PlayerData = GetPlayerData(),
            EnemyDatas = GetEnemyDatas(),
        };

        // マスタクライアント情報更新リクエスト
        await RoomModel.Instance.UpdateMasterClientAsync(masterClientData);
    }

    /// <summary>
    /// プレイヤーの情報更新
    /// </summary>
    async void UpdatePlayerData()
    {
        var playerData = GetPlayerData();

        // プレイヤー情報更新リクエスト
        await RoomModel.Instance.MovePlayerAsync(playerData);
    }

    /// <summary>
    /// 新たな敵をリストに追加する
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
    /// プレイヤーの更新の通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerData"></param>
    void OnMovePlayer(PlayerData playerData)
    {
        
    }

    /// <summary>
    /// マスタークライアントの更新通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="masterClientData"></param>
    void OnUpdateMasterClient(MasterClientData masterClientData)
    {

    }
}
