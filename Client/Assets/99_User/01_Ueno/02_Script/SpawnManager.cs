//----------------------------------------------------
// 敵生成クラス
// Author : Souma Ueno
//----------------------------------------------------
using DG.Tweening;
using NUnit.Framework;
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
    #region 敵生成条件
    [SerializeField] Vector2 spawnRange;
    [SerializeField] float spawnRangeOffset;
    #endregion

    #region 敵生成関連
    [SerializeField] int maxSpawnCnt; // マックススポーン回数
    public int MaxSpawnCnt { get { return maxSpawnCnt; } }
    [SerializeField] int knockTermsNum;      // ボスのエネミーの撃破数条件
    public int KnockTermsNum { get { return knockTermsNum; } }
    [SerializeField] float spawnProbability = 0.05f; // 5%の確率 (0.0から1.0の間で指定)
    int fivePercentOfMaxFloor;
    List<Vector3> enemySpawnPosList = new List<Vector3>();
    #endregion

    #region ステージ情報
    [SerializeField] Transform stageMin;             // リスポーン範囲A
    [SerializeField] Transform stageMax;             // リスポーン範囲B
    [SerializeField] Transform minTerminalRespawn;       // ターミナルリスポーン範囲
    [SerializeField] Transform maxTerminalRespawn;       // ターミナルリスポーン範囲

    #region 外部参照
    public Transform StageMinPoint { get { return stageMin; } }
    public Transform StageMaxPoint { get { return stageMax; } }
    #endregion

    #endregion

    #region 敵関連
    [SerializeField] List<GameObject> enemyPrefabs;      // エネミーのプレファブリスト
    [SerializeField] List<EnumManager.ENEMY_TYPE> emitEnemyTypes;   // 生成対象の敵の種類
    public List<EnumManager.ENEMY_TYPE> EmitEnemyTypes { get { return emitEnemyTypes; } }

    [SerializeField] Dictionary<EnumManager.ENEMY_TYPE, GameObject> idEnemyPrefabPairs;
    public Dictionary<EnumManager.ENEMY_TYPE, GameObject> IdEnemyPrefabPairs { get { return idEnemyPrefabPairs; } }

    int eliteEnemyCnt;
    List<GameObject> terminalEnemyList = new List<GameObject>();
    public List<GameObject> TerminalEnemyList { get { return terminalEnemyList; } }
    #endregion

    #region ボス関連
    GameObject boss;            // ボス
    public GameObject Boss { get { return boss; }}
    [SerializeField] GameObject bossTerminal;
    [SerializeField] ENEMY_TYPE bossId;
    bool isSpawnBoss;           // ボスが生成されたかどうか
    public bool IsSpawnBoss { get {  return isSpawnBoss; } set {  isSpawnBoss = value; } }
    #endregion

    int crashNum = 0; 　　　　　// 撃破数
    public int CrashNum { get { return crashNum; } set { crashNum = value; } }

    CharacterManager characterManager;

    private static SpawnManager instance;

    public static SpawnManager Instance
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

        StartCoroutine(WaitAndStartCoroutine(5f));
    }

    private void OnDisable()
    {
        // RoomModelが存在するなら、登録済みのアクションを解除
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSpawndEnemy -= this.OnSpawnEnemy;
    }

    IEnumerator SpawnCoroutin()
    {
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
                    if (!isSpawnBoss)
                    {
                        foreach (var player in CharacterManager.Instance.PlayerObjs.Values)
                        {
                            if (characterManager.Enemies.Count > maxSpawnCnt) break;
                            if (!player) continue;
                            if (characterManager.Enemies.Count < maxSpawnCnt / 2)
                            {// 敵が100体いない場合
                                GenerateEnemy(Random.Range(7, 11), player.transform.position);
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

    IEnumerator WaitAndStartCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SpawnCoroutin());
    }

    /// <summary>
    /// 敵のプレファブ情報をまとめる
    /// </summary>
    void SetEnemyPrefabList()
    {
        idEnemyPrefabPairs = new Dictionary<EnumManager.ENEMY_TYPE, GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            Debug.Log(prefab.GetComponent<EnemyBase>().EnemyTypeId + "：" + prefab.name);
            idEnemyPrefabPairs.Add(prefab.GetComponent<EnemyBase>().EnemyTypeId, prefab);
        }
    }

    /// <summary>
    /// 敵のスポーン可能範囲判定処理
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    public (Vector3 minRange, Vector3 maxRange) CreateEnemyTerminalSpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < minTerminalRespawn.position.y)
        {
            minRange.y = minTerminalRespawn.position.y;
        }

        if (minPoint.x < minTerminalRespawn.position.x)
        {
            minRange.x = minTerminalRespawn.position.x;
        }

        if (maxPoint.y > maxTerminalRespawn.position.y)
        {
            maxRange.y = maxTerminalRespawn.position.y;
        }

        if (maxPoint.x > maxTerminalRespawn.position.x)
        {
            maxRange.x = maxTerminalRespawn.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// 敵生成の位置決定処理
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    public Vector3? GenerateEnemySpawnPosition(Vector3 minRange, Vector3 maxRange, EnemyBase enemyBase)
    {
        // 試行回数
        int loopMax = 100;

        for (int i = 0; i < loopMax; i++)
        {
            int seed = System.DateTime.Now.Millisecond + i;
            Random.InitState(seed);  // Unityの乱数にシードを設定

            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector2? pos = IsGroundCheck(spawnPos);
            if (pos != null && !enemySpawnPosList.Contains(spawnPos))
            {
                // listの中にない場合、リストにadd
                enemySpawnPosList.Add(spawnPos);

                LayerMask mask = LayerMask.GetMask("Default");

                Vector2 result = (Vector2)pos;

                result.y += enemyBase.SpawnGroundOffset;

                if (!Physics2D.OverlapCircle(new Vector2(result.x, result.y + 1), 0.8f, mask))
                {
                    return result;
                }
            }
        }

        return null;
    }

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

            Vector2 spawnRight = playerPos + Vector2.right * spawnRangeOffset;
            Vector2 spawnLeft = playerPos + Vector2.left * spawnRangeOffset;

            Vector2 minSpawnRight = spawnRight - spawnRight / 2;
            Vector2 maxSpawnRight = spawnRight + spawnRight / 2;

            Vector2 minSpawnLeft = spawnLeft - spawnLeft / 2;
            Vector2 maxSpawnLeft = spawnLeft + spawnLeft / 2;

            Vector3? spawnRightPosCandidate = null, spawnLeftPosCandidate = null, spawnPos = null;

            spawnRightPosCandidate = GenerateEnemySpawnPosition(minSpawnRight, maxSpawnRight, enemyBase);
            spawnLeftPosCandidate = GenerateEnemySpawnPosition(minSpawnLeft, maxSpawnLeft, enemyBase);

            if (spawnLeftPosCandidate != null && spawnRightPosCandidate != null)
            {
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
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // 一旦このまま
                spawnEnemyDatas.Add(CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType));
            }
        }
        // 生成スポーンリスト初期化
        enemySpawnPosList.Clear();
        SpawnEnemyRequest(spawnEnemyDatas.ToArray());
    }

    /// <summary>
    /// 端末操作時の敵生成処理
    /// </summary>
    public void TerminalGenerateEnemy(int num, Vector2 minPos, Vector2 maxPos)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
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

            // ランダムな位置を生成
            var spawnPostions = CreateEnemyTerminalSpawnPosition(minPos, maxPos);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                Vector3 scale = Vector3.one;    // 一旦このまま
                var spawnData = CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                SpawnEnemyRequest(null, spawnData);

                // 端末から出た敵をリストに追加
                terminalEnemyList = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByTerminal);

                enemyCnt++;
            }
        }
    }

    /// <summary>
    /// 床チェック
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <returns></returns>
    private Vector2? IsGroundCheck(Vector3 rayOrigin)
    {
        LayerMask mask = LayerMask.GetMask("Default");

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, mask);

        Debug.DrawRay((Vector2)rayOrigin, Vector2.down * hit.distance, Color.red);
        if (hit && hit.collider.gameObject.CompareTag("ground"))
        {
            return hit.point;
        }
        else
        {
            return null;
        }
    }

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

        // プレイヤーの向きを算出
        var playerPos = FetchNearObjectWithTag("Player");
        float scaleX = playerPos.position.x - entryData.Position.x;
        scaleX = scaleX >= 0 ? 1 : -1;
        var enemyScale = new Vector3(scaleX, 1, 1);

        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = Guid.NewGuid(),
            Position = entryData.Position,
            Scale = enemyScale,
            SpawnType = spawnType,
            EliteType = eliteType,
        };
    }


    public SpawnEnemyData CreateTerminalSpawnEnemyData(EnemySpawnEntry entryData, SPAWN_ENEMY_TYPE spawnType)
    {
        if (entryData.EnemyType == null)
        {
            Debug.LogWarning("entryData.EnemyTypeがnullだったため、データの生成を中断しました。");
            return null;
        }
        ENEMY_ELITE_TYPE eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);

        Debug.Log(eliteType);

        // プレイヤーの向きを算出
        var playerPos = FetchNearObjectWithTag("Player");
        float scaleX = playerPos.position.x - entryData.Position.x;
        scaleX = scaleX >= 0 ? 1 : -1;
        var enemyScale = new Vector3(scaleX, 1, 1);

        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = Guid.NewGuid(),
            Position = entryData.Position,
            Scale = enemyScale,
            SpawnType = spawnType,
            EliteType = eliteType,
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

        // ID設定(ローカル用)
        //spawnEnemyData.UniqueId = RoomModel.Instance ? spawnEnemyData.UniqueId : CharacterManager.Instance.Enemies.Count;

        // 敵の生成
        var prefab = idEnemyPrefabPairs[spawnEnemyData.TypeId];
        var position = spawnEnemyData.Position;
        var scale = spawnEnemyData.Scale;
        var eliteType = spawnEnemyData.EliteType;
        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
        //if (LevelManager.Instance.GameLevel > 0)
        //{
        //    enemyObj.GetComponent<CharacterBase>().ApplyStatusModifierByRate(10 * ((int)LevelManager.Instance.GameLevel));
        //}
        enemyObj.transform.localScale = scale;
        enemyObj.GetComponent<EnemyBase>().PromoteToElite(eliteType);
        enemyObj.GetComponent<EnemyBase>().UniqueId = spawnEnemyData.UniqueId;
        CharacterManager.Instance.AddEnemiesToList(new SpawnedEnemy(spawnEnemyData.UniqueId, enemyObj, enemyObj.GetComponent<EnemyBase>(), spawnEnemyData.SpawnType));

        return enemyObj;
    }

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

    /// <summary>
    /// ターミナル用敵生成リクエスト
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnTerminalEnemyRequest(params SpawnEnemyData[] spawnDatas)
    {
        List<GameObject> enemyObj = new List<GameObject>();

        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatasにnullの要素が見つかったため、敵の生成を中断しました。");
            return;
        }

        // ここで敵の生成リクエスト
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // SpawnEnemyAsyncを呼び出す
            await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            enemyObj.Add(SpawnEnemy(spawnEnemyData));
        }
    }

    /// <summary>
    /// ターミナル用敵生成リクエスト
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnTerminalEnemyRequest(Terminal terminal, params SpawnEnemyData[] spawnDatas)
    {
        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatasにnullの要素が見つかったため、敵の生成を中断しました。");
            return;
        }

        // ここで敵の生成リクエスト
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // SpawnEnemyAsyncを呼び出す
            await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            GameObject enemy = SpawnEnemy(spawnEnemyData);
            enemy.GetComponent<EnemyBase>().TerminalManager = terminal;
            terminal.TerminalSpawnList.Add(enemy);
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

    private void OnDrawGizmos()
    {
        if (CharacterManager.Instance && CharacterManager.Instance.PlayerObjSelf)
        {
            var player = CharacterManager.Instance.PlayerObjSelf;
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // 右
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // 左
        }
    }

    [ContextMenu("SpawnBoss")]
    public void SpawnBoss()
    {
        if (!isSpawnBoss)
        {
            EnemyBase bossEnemy = idEnemyPrefabPairs[bossId].GetComponent<EnemyBase>();

            SpawnEnemyData spawnEnemyDatas = new SpawnEnemyData();

            int childrenCnt = bossTerminal.transform.childCount;

            List<Transform> children = new List<Transform>();

            for (int i = 0; i < childrenCnt; i++)
            {
                children.Add(bossTerminal.transform.GetChild(i));
            }
            ENEMY_TYPE enemyType = ENEMY_TYPE.Worm;

            Vector3? spawnPos =
                GenerateEnemySpawnPosition(children[0].position, children[1].position, bossEnemy);

            if (spawnPos != null)
            {// 返り値がnullじゃないとき
                boss = idEnemyPrefabPairs[bossId];
                
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // 一旦このまま
                spawnEnemyDatas = CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);
            }

            isSpawnBoss = true;

            UIManager.Instance.DisplayBossUI();

            SpawnEnemyRequest(spawnEnemyDatas);
        }
    }

    /// <summary>
    /// １番近いオブジェクトを取得する
    /// </summary>
    /// <param name="tagName">取得したいtagName</param>
    /// <returns>最小距離の指定Obj</returns>
    private Transform FetchNearObjectWithTag(string tagName)
    {
        // 該当タグが1つしか無い場合はそれを返す
        var targets = GameObject.FindGameObjectsWithTag(tagName);
        if (targets.Length == 1) return targets[0].transform;
        GameObject result = null;               // 返り値
        var minTargetDistance = float.MaxValue; // 最小距離
        foreach (var target in targets)
        {
            // 前回計測したオブジェクトよりも近くにあれば記録
            var targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (!(targetDistance < minTargetDistance)) continue;
            minTargetDistance = targetDistance;
            result = target.transform.gameObject;
        }
        // 最後に記録されたオブジェクトを返す
        return result?.transform;
    }

}