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
using DG.Tweening;
using UnityEditor.MemoryProfiler;
using UnityEngine.Playables;

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

    /// <summary>
    /// プレイヤーのリスト
    /// </summary>
    public Dictionary<Guid, GameObject> PlayerObjs { get {  return playerObjs; } }
    #endregion

    #region 敵関連
    Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();

    /// <summary>
    /// 現在のステージで生成した敵のリスト
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
    /// シーン遷移したときに通知関数呼び出しを止める
    /// </summary>
    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnMovePlayerSyn -= this.OnMovePlayer;                    //シーン遷移した場合に通知関数をモデルから解除
        RoomModel.Instance.OnUpdateMasterClientSyn -= this.OnUpdateMasterClient;    //シーン遷移した場合に通知関数をモデルから解除
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
            playerObjs.Add(Guid.Empty, playerObjSelf);
            return;
        }
        DestroyExistingPlayer();
        GenerateCharacters();
        isAwake = true;

        //プレイヤーの更新通知時に呼ぶ
        RoomModel.Instance.OnMovePlayerSyn += this.OnMovePlayer;
        //マスタークライアントの更新通知時に呼ぶ
        RoomModel.Instance.OnUpdateMasterClientSyn += this.OnUpdateMasterClient;
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        StartCoroutine("UpdateCoroutine");
    }

    /// <summary>
    /// キャラクターの情報更新呼び出し用コルーチン
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
    /// 新たな敵をリストに追加する
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemies(int id, GameObject newEnemies)
    {
        enemies.Add(id, newEnemies);
    }

    /// <summary>
    /// キャラクターの情報を更新する
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

        // マスタークライアントの場合、キャラクターが動けるようにする
        if (RoomModel.Instance.IsMaster && !character.enabled)
        {
            character.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            character.enabled = true;
        }
    }

    /// <summary>
    /// プレイヤー情報取得
    /// </summary>
    /// <returns></returns>
    PlayerData GetPlayerData()
    {
        if (!playerObjs.ContainsKey(RoomModel.Instance.ConnectionId)) return null;
        Debug.Log("キャラクター："+RoomModel.Instance.ConnectionId);
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
    async void UpdateMasterDataRequest()
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
    async void UpdatePlayerDataRequest()
    {
        var playerData = GetPlayerData();

        // プレイヤー情報更新リクエスト
        await RoomModel.Instance.UpdatePlayerAsync(playerData);
    }

    /// <summary>
    /// プレイヤーの更新の通知
    /// </summary>
    /// <param name="playerData"></param>
    void OnMovePlayer(PlayerData playerData)
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
        UpdateCharacter(masterClientData.PlayerData, player);

        // 敵の情報更新
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
