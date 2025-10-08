//----------------------------------------------------
// 敵生成クラス
// Author : Souma Ueno
//----------------------------------------------------
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region プレイヤーを軸にした敵の生成範囲
    [Foldout("プレイヤーを軸にした敵の生成範囲")]
    [SerializeField] Vector2 spawnRange;    // 敵の生成範囲
    [Foldout("プレイヤーを軸にした敵の生成範囲")]
    [SerializeField] float spawnRangeOffset;    // 敵の生成範囲に対するオフセット
    #endregion

    #region 敵生成関連

    [Foldout("敵の生成関連")]
    [SerializeField] int maxSpawnCnt; // マックススポーン回数
    public int MaxSpawnCnt { get { return maxSpawnCnt; } }

    [Foldout("敵の生成関連")]
    [Tooltip("ボスを生成するための条件数(敵を撃破する度にカウント)")]
    [SerializeField] int knockTermsNum;
    public int KnockTermsNum { get { return knockTermsNum; } }

    [Foldout("敵の生成関連")]
    [Tooltip("生成する敵がエリート個体になる確率")]
    [SerializeField] float spawnProbability = 0.05f; // 5%の確率 (0.0から1.0の間で指定)
    int fivePercentOfMaxFloor;

    int spawnCount;
    int enemyCnt;
    int eliteEnemyCnt;
    List<Vector3> enemySpawnPosList = new List<Vector3>();  // 一度に生成する座標のリスト
    LayerMask terrainLayerMask; // 地形のマスク(ギミック含む)
    #endregion

    #region ステージ情報
    [Foldout("ステージ情報関連")]
    [SerializeField] Transform stageMin;
    [Foldout("ステージ情報関連")]
    [SerializeField] Transform stageMax;

    #region 外部参照
    public Transform StageMinPoint { get { return stageMin; } }
    public Transform StageMaxPoint { get { return stageMax; } }
    #endregion

    #endregion

    #region 生成する敵の情報関連

    [Foldout("生成する敵の情報関連")]
    [SerializeField] List<GameObject> enemyPrefabs;      // エネミーのプレファブリスト

    [Foldout("生成する敵の情報関連")]
    [Tooltip("生成対象の敵の種類")]
    [SerializeField] List<ENEMY_TYPE> emitEnemyTypes;   // 生成対象の敵の種類
    public List<ENEMY_TYPE> EmitEnemyTypes { get { return emitEnemyTypes; } }

    // emitEnemyTypesとenemyPrefabsを組み合わせたリスト
    Dictionary<ENEMY_TYPE, GameObject> idEnemyPrefabPairs;
    public Dictionary<ENEMY_TYPE, GameObject> IdEnemyPrefabPairs { get { return idEnemyPrefabPairs; } }

    List<GameObject> terminalEnemyList = new List<GameObject>();
    public List<GameObject> TerminalEnemyList { get { return terminalEnemyList; } }
    #endregion

    #region ボス関連

    [Foldout("ボス関連")]
    [SerializeField] Vector2 spawnBossPoint;

    [Foldout("ボス関連")]
    [SerializeField] ENEMY_TYPE bossId;

    bool isBossActive;           // ボスが生成されたかどうか
    public bool IsSpawnBoss { get {  return isBossActive; } set {  isBossActive = value; } }
    #endregion

    #region 敵撃破関連
    int crashNum = 0;   // 撃破数
    public int CrashNum { get { return crashNum; } set { crashNum = value; } }
    #endregion

    #region シングルトン
    private static SpawnManager instance;

    public static SpawnManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region その他
    CharacterManager characterManager;
    #endregion

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

        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick") | LayerMask.GetMask("Scaffold");

        // RoomModelが実行してない場合はここから下は実行しない
        if (RoomModel.Instance == null) return;

        // ルームモデルでOnSpawndEnemy実行時に、この中でOnSpawnEnemyを実行する
        RoomModel.Instance.OnSpawndEnemy += this.OnSpawnEnemy;
    }

    private void Start()
    {
        SetEnemyPrefabList();

        characterManager = CharacterManager.Instance;

        // 敵生成上限の5%を取得
        fivePercentOfMaxFloor = (int)((float)maxSpawnCnt * spawnProbability);

        StartCoroutine(SpawnCoroutin(2f));
    }

    private void OnDisable()
    {
        // RoomModelが存在するなら、登録済みのアクションを解除
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSpawndEnemy -= this.OnSpawnEnemy;
    }

    /// <summary>
    /// 一定間隔で敵生成処理呼び出し
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator SpawnCoroutin(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            if (GameManager.Instance.IsGameStart)
            {
                if (crashNum >= knockTermsNum && IsSpawnBoss)
                {
                    SpawnBoss();
                }

                if (characterManager.Enemies.Count < maxSpawnCnt && !GameManager.Instance.IsBossDead)
                {// スポーン回数が限界に達しているか
                    if (!isBossActive)
                    {
                        enemyCnt = characterManager.Enemies.Count;

                        foreach (var player in CharacterManager.Instance.PlayerObjs.Values)
                        {
                            if (enemyCnt > maxSpawnCnt) break;
                            if (!player) continue;
                            if (enemyCnt < maxSpawnCnt / 2)
                            {// 敵が100体いない場合
                                GenerateEnemy(Random.Range(3, 6), player.transform.position);
                            }
                            else
                            {// いる場合
                                GenerateEnemy(1, player.transform.position);
                            }

                        }
                    }
                }
            }

            yield return new WaitForSeconds(10);
        }
    }

    /// <summary>
    /// 敵のプレファブ情報をまとめる
    /// </summary>
    void SetEnemyPrefabList()
    {
        idEnemyPrefabPairs = new Dictionary<ENEMY_TYPE, GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            Debug.Log(prefab.GetComponent<EnemyBase>().EnemyTypeId + "：" + prefab.name);
            idEnemyPrefabPairs.Add(prefab.GetComponent<EnemyBase>().EnemyTypeId, prefab);
        }
    }

    #region 敵生成関連

    /// <summary>
    /// 敵生成処理
    /// </summary>
    public void GenerateEnemy(int num,Vector2 playerPos)
    {
        // マスタクライアント以外は処理をしない
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        List<SpawnEnemyData> spawnEnemyDatas = new List<SpawnEnemyData>();
        for (int i = 0; i < num; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // 乱数のシード値を更新

            if (characterManager.Enemies.Count > maxSpawnCnt)
            {
                return; 
            }

            // 生成する敵の抽選
            var emitResult = EmitEnemy(emitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("生成する敵の抽選結果がnullのため、以降の処理をスキップします。");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;
            EnemyBase enemyBase = idEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            // 対象のプレイヤーを軸とした生成範囲取得
            var spawnRightMinPoint = (playerPos + Vector2.right * spawnRangeOffset) - spawnRange / 2;
            var spawnRightMaxPoint = (playerPos + Vector2.right * spawnRangeOffset) + spawnRange / 2;
            var spawnLeftMinPoint = (playerPos + Vector2.left * spawnRangeOffset) - spawnRange / 2;
            var spawnLeftMaxPoint = (playerPos + Vector2.left * spawnRangeOffset) + spawnRange / 2;

            // ランダムな生成可能の座標を抽選
            Vector3? spawnRightPosCandidate = null, spawnLeftPosCandidate = null, spawnPos = null;
            spawnRightPosCandidate = EmitEnemySpawnPosition(spawnRightMinPoint, spawnRightMaxPoint, enemyBase);
            spawnLeftPosCandidate = EmitEnemySpawnPosition(spawnLeftMinPoint, spawnLeftMaxPoint, enemyBase);

            if (spawnLeftPosCandidate != null && spawnRightPosCandidate != null)
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // 乱数のシード値を更新
                int rand = Random.Range(0, 2);

                if(rand == 0)
                {
                    spawnPos = spawnRightPosCandidate;
                }
                else
                {
                    spawnPos = spawnLeftPosCandidate;
                }
            }
            else if (spawnLeftPosCandidate != null || spawnRightPosCandidate != null)
            {
                spawnPos = spawnRightPosCandidate == null ? spawnLeftPosCandidate : spawnRightPosCandidate;
            }

            // 生成座標が確定している場合は敵を生成する
            if (spawnPos != null)
            {
                var spawnType = SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // 一旦このまま
                spawnEnemyDatas.Add(CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType));

                // スポーンカウント増やす
                spawnCount++;
            }
        }
        // 生成スポーンリスト初期化
        enemySpawnPosList.Clear();
        SpawnEnemyRequest(spawnEnemyDatas.ToArray());
    }

    /// <summary>
    /// 端末操作時の敵生成処理
    /// </summary>
    public void TerminalGenerateEnemy(int generateNum, int termID , Vector2 minPos, Vector2 maxPos, bool elite)
    {
        // マスタクライアント以外は処理をしない
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        int enemyCnt = 0;

        while (enemyCnt < generateNum)
        {
            // 生成する敵の抽選
            var emitResult = EmitEnemy(emitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("生成する敵の抽選結果がnullのため、以降の処理をスキップします。");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;

            EnemyBase enemyBase = idEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            Vector3? spawnPos = EmitEnemySpawnPosition(minPos, maxPos, enemyBase);

            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                Vector3 scale = Vector3.one;    // 一旦このまま
                var spawnData = CreateTerminalSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType, termID , elite);

                SpawnEnemyRequest(spawnData);

                // 端末から出た敵をリストに追加
                terminalEnemyList = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByTerminal);

                enemyCnt++;
            }
        }
    }

    /// <summary>
    /// ボスの生成処理
    /// </summary>
    public void SpawnBoss()
    {
        if (!isBossActive)
        {
            EnemyBase bossEnemy = idEnemyPrefabPairs[bossId].GetComponent<EnemyBase>();
            List<EnemySpawnEntry> entrys = new List<EnemySpawnEntry>()
                {
                    new EnemySpawnEntry(bossId, spawnBossPoint, bossEnemy.transform.localScale)
                };

            #region サーバーにワームの各パーツの情報を登録するための処理

            // ワームを生成する場合、ワームの各パーツも生成情報に含める (※SpawnEnemy()で実際に生成はしない)
            List<FullMetalBody> bodys = new List<FullMetalBody>(
                bossEnemy.transform.gameObject.GetComponentsInChildren<FullMetalBody>(true));
            if (bodys.Count > 0)
            {
                foreach (var body in bodys)
                {
                    // 事前に設定されてある識別用ID(body.UniqueId)もデータに追加する
                    entrys.Add(new EnemySpawnEntry(ENEMY_TYPE.MetalBody, Vector3.zero, Vector3.zero, body.UniqueId));
                }
            }
            #endregion

            var spawnEnemyDatas = CreateSpawnEnemyDatas(entrys, SPAWN_ENEMY_TYPE.ByManager, false);

            isBossActive = true;

            SpawnEnemyRequest(spawnEnemyDatas.ToArray());
        }
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// 敵生成の座標抽選処理
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    Vector3? EmitEnemySpawnPosition(Vector3 minRange, Vector3 maxRange, EnemyBase enemyBase)
    {
        // 試行回数
        int loopMax = 100;

        for (int i = 0; i < loopMax; i++)
        {
            int seed = System.DateTime.Now.Millisecond + i;
            Random.InitState(seed);  // Unityの乱数にシードを設定

            float rndX = Random.Range(minRange.x, maxRange.x);
            float rndY = Random.Range(minRange.y, maxRange.y);
            Vector3 rndPos = new Vector3(rndX, rndY);

            Vector2? groundPos = IsGroundCheck(rndPos); // 地面の上に生成可能かチェック
            if (groundPos != null)
            {
                Vector2 spawnPos = (Vector2)groundPos + Vector2.up * enemyBase.SpawnGroundOffset;

                // 障害物が重なっていない かつ 生成座標が重複していない場合は成功
                if (!enemyBase.IsOverlappingObstacle(spawnPos) && !enemySpawnPosList.Contains(spawnPos))
                {
                    enemySpawnPosList.Add(spawnPos);
                    return spawnPos;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 床チェック
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <returns></returns>
    Vector2? IsGroundCheck(Vector3 rayOrigin)
    {
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, terrainLayerMask);

        //Debug.DrawRay((Vector2)rayOrigin, Vector2.down * hit.distance, Color.red);
        if(hit && hit.transform.tag != "ClearWall") return hit.point;
        else return null;
    }

    #endregion

    #region 抽選処理

    /// <summary>
    /// 敵の出現確率の抽出
    /// </summary>
    /// <param name="probs"></param>
    /// <returns></returns>
    public int Choose(float[] probs)
    {
        float total = 0;

        //配列の要素を代入して重みの計算
        foreach (float elem in probs)
        {
            total += elem;
        }

        //重みの総数に0から1.0の乱数をかけて抽選を行う
        float randomPoint = Random.value * total;

        //iが配列の最大要素数になるまで繰り返す
        for (int i = 0; i < probs.Length; i++)
        {
            //ランダムポイントが重みより小さいなら
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                //ランダムポイントが重みより大きいならその値を引いて次の要素へ
                randomPoint -= probs[i];
            }
        }

        //乱数が１の時、配列数の-１＝要素の最後の値をChoose配列に戻している
        return probs.Length - 1;
    }

    /// <summary>
    /// 生成する敵の抽選処理
    /// </summary>
    public ENEMY_TYPE? EmitEnemy(params ENEMY_TYPE[] types)
    {
        // 合計の重みを取得
        int tatalWeight = 0;
        List<EnemyBase> enemies = new List<EnemyBase>();
        foreach(var type in types)
        {
            if (idEnemyPrefabPairs.ContainsKey(type))
            {
                var enemy = idEnemyPrefabPairs[type].GetComponent<EnemyBase>();
                enemies.Add(enemy);
                tatalWeight += enemy.SpawnWeight;
            }
        }

        // キャラクターIDを基準に昇順にソート
        enemies.Sort((a, b) => a.EnemyTypeId.CompareTo(b.EnemyTypeId));

        EnumManager.ENEMY_TYPE? entryType = null;
        int emitRnd = Random.Range(1, tatalWeight + 1);
        int currentWeight = 0;
        foreach(EnemyBase enemy in enemies)
        {
            currentWeight += enemy.SpawnWeight;
            if (emitRnd <= currentWeight)
            {
                entryType = enemy.EnemyTypeId;
                break;
            }
        }
        return entryType;
    }

    #endregion

    #region スポーンデータ作成

    /// <summary>
    /// 複数の生成する敵のデータ作成
    /// </summary>
    /// <param name="entryList"></param>
    /// <param name="spawnType"></param>
    /// <param name="canPromoteToElite"></param>
    /// <returns></returns>
    public List<SpawnEnemyData> CreateSpawnEnemyDatas(List<EnemySpawnEntry> entryList, SPAWN_ENEMY_TYPE spawnType, bool canPromoteToElite = true)
    {
        List<SpawnEnemyData> spawnDatas = new List<SpawnEnemyData>();
        foreach (EnemySpawnEntry entry in entryList)
        {
            spawnDatas.Add(CreateSpawnEnemyData(entry, spawnType, canPromoteToElite));
        }
        return spawnDatas;
    }

    /// <summary>
    /// 生成する敵のデータを作成
    /// </summary>
    /// <param name="enemyPrefabs"></param>
    /// <param name="positions"></param>
    /// <param name="canPromoteToElite"></param>
    /// <returns></returns>
    public SpawnEnemyData CreateSpawnEnemyData(EnemySpawnEntry entryData, SPAWN_ENEMY_TYPE spawnType, bool canPromoteToElite = true)
    {
        if (entryData.EnemyType == null)
        {
            Debug.LogWarning("entryData.EnemyTypeがnullだったため、データの生成を中断しました。");
            return null;
        }
        ENEMY_ELITE_TYPE eliteType = ENEMY_ELITE_TYPE.None;

        if (canPromoteToElite && fivePercentOfMaxFloor > eliteEnemyCnt
            && Random.value < spawnProbability)
        {
            eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);

            eliteEnemyCnt++;

            Debug.Log(eliteType);
        }
        else if (canPromoteToElite)
        {
            eliteType = ENEMY_ELITE_TYPE.None;
        }

        // 返すデータ作成
        string uniqueId = entryData.PresetId == "" ? Guid.NewGuid().ToString() : entryData.PresetId;    // 事前に識別用IDが設定されていない場合は生成する
        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = uniqueId,
            Position = entryData.Position,
            Scale = entryData.Scale,
            SpawnType = spawnType,
            EliteType = eliteType,
            TerminalID = -1,
        };
    }

    #endregion

    #region 生成実行処理

    /// <summary>
    /// 端末用敵生成処理
    /// </summary>
    /// <param name="entryData"></param>
    /// <param name="spawnType"></param>
    /// <param name="canPromoteToElite"></param>
    /// <param name="termID"></param>
    /// <returns></returns>
    public SpawnEnemyData CreateTerminalSpawnEnemyData(EnemySpawnEntry entryData, SPAWN_ENEMY_TYPE spawnType, int termID, bool canPromoteToElite = true)
    {
        if (entryData.EnemyType == null)
        {
            Debug.LogWarning("entryData.EnemyTypeがnullだったため、データの生成を中断しました。");
            return null;
        }
        ENEMY_ELITE_TYPE eliteType = ENEMY_ELITE_TYPE.None;

        if (canPromoteToElite)
        {
            eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);

            Debug.Log(eliteType);
        }
        else if (!canPromoteToElite)
        {
            eliteType = ENEMY_ELITE_TYPE.None;
        }

        // 返すデータ作成
        string uniqueId = entryData.PresetId == "" ? Guid.NewGuid().ToString() : entryData.PresetId;    // 事前に識別用IDが設定されていない場合は生成する
        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = uniqueId,
            Position = entryData.Position,
            Scale = entryData.Scale,
            SpawnType = spawnType,
            EliteType = eliteType,
            TerminalID = termID
        };
    }

    /// <summary>
    /// 敵の生成実行
    /// </summary>
    /// <param name="spawnEnemyData"></param>
    /// <returns></returns>
    GameObject SpawnEnemy(SpawnEnemyData spawnEnemyData)
    {
        if (spawnEnemyData == null)
        {
            Debug.LogWarning("nullの要素が見つかったため、敵の生成を中断しました。");
            return null;
        }
        else if (spawnEnemyData.TypeId == ENEMY_TYPE.MetalBody) return null;

        // 敵の生成
        var prefab = idEnemyPrefabPairs[spawnEnemyData.TypeId];
        var position = spawnEnemyData.Position;
        var scale = spawnEnemyData.Scale;
        var eliteType = spawnEnemyData.EliteType;
        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
        enemyObj.transform.localScale = scale;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        enemy.PromoteToElite(eliteType);
        enemy.UniqueId = spawnEnemyData.UniqueId;

        var spawnedEnemy = (spawnEnemyData.SpawnType == SPAWN_ENEMY_TYPE.ByTerminal)? 
            new SpawnedEnemy(spawnEnemyData.UniqueId, enemyObj, enemyObj.GetComponent<EnemyBase>(), spawnEnemyData.SpawnType, spawnEnemyData.TerminalID) : 
            new SpawnedEnemy(spawnEnemyData.UniqueId, enemyObj, enemyObj.GetComponent<EnemyBase>(), spawnEnemyData.SpawnType);

        CharacterManager.Instance.AddEnemiesToList(spawnedEnemy);

        #region ワームの各パーツ用の処理
        if (enemy.EnemyTypeId == ENEMY_TYPE.FullMetalWorm)
        {
            // ワームを生成する際、ワームの各パーツも生成した敵のリストに含める
            List<FullMetalBody> bodys = new List<FullMetalBody>(enemyObj.GetComponentsInChildren<FullMetalBody>(true));
            foreach (var body in bodys)
            {
                CharacterManager.Instance.AddEnemiesToList(new SpawnedEnemy(body.UniqueId, body.gameObject, body, spawnEnemyData.SpawnType));
            }
        }
        #endregion

        if (enemy.GetComponent<EnemyBase>().IsBoss)
        {
            GameManager.Instance.PlayBossBGM();
            UIManager.Instance.DisplayBossUI();
        }

        return enemyObj;
    }

    #endregion

    #region デバック用

    private void OnDrawGizmos()
    {
        if (CharacterManager.Instance && CharacterManager.Instance.PlayerObjSelf)
        {
            var player = CharacterManager.Instance.PlayerObjSelf;
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // 右
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // 左
        }
    }

    #endregion

    #region リアルタイム同期用

    #region リクエスト関連

    /// <summary>
    /// 敵生成リクエスト
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnEnemyRequest(params SpawnEnemyData[] spawnDatas)
    {
        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatasにnullの要素が見つかったため、敵の生成を中断しました。");
            return;
        }

        // マスタクライアントのみ敵の生成をリクエスト
        if (RoomModel.Instance)
        {
            if (RoomModel.Instance.IsMaster) await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        // ローカル用
        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            SpawnEnemy(spawnEnemyData);
        }
    }
    #endregion

    #region 通知関連

    /// <summary>
    /// 敵の生成通知
    /// </summary>
    /// <param name="spawnEnemyDatas"></param>
    void OnSpawnEnemy(List<SpawnEnemyData> spawnEnemyDatas)
    {
        foreach(SpawnEnemyData spawnEnemyData in spawnEnemyDatas)
        {
            SpawnEnemy(spawnEnemyData);
        }
    }

    #endregion

    #endregion
}