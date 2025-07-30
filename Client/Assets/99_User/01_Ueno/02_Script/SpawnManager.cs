//----------------------------------------------------
// 敵生成クラス
// Author : Souma Ueno
//----------------------------------------------------
using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region 敵生成条件
    [SerializeField] Vector2 spawnRange;
    [SerializeField] float spawnRangeOffset;
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
    [SerializeField] Dictionary<EnumManager.ENEMY_TYPE, GameObject> idEnemyPrefabPairs;
    float[] enemyWeights;
    int eliteEnemyCnt;
    #endregion

    #region 削除予定
    [SerializeField] float distMinSpawnPos;              // 生成しない範囲
    [SerializeField] float xRadius;                      // 生成範囲のx半径
    [SerializeField] float yRadius;                      // 生成範囲のy半径
    public List<GameObject> EnemiesByTerminal { get { return null; } }
    #endregion

    [SerializeField] GameObject player;
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
        player = CharacterManager.Instance.PlayerObjSelf;
        enemyWeights = new float[idEnemyPrefabPairs.Count];

        // 割合
        enemyWeights[0] = 24; // ドローン
        enemyWeights[1] = 76; // いぬ
    }

    private void OnDisable()
    {
        // 実行終了時にモデルの共有を切る
        RoomModel.Instance.OnSpawndEnemy -= this.OnSpawnEnemy;
    }

    /// <summary>
    /// 敵のプレファブ情報をまとめる
    /// </summary>
    void SetEnemyPrefabList()
    {
        idEnemyPrefabPairs = new Dictionary<EnumManager.ENEMY_TYPE, GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            Debug.Log((EnumManager.ENEMY_TYPE)prefab.GetComponent<CharacterBase>().CharacterId + "：" + prefab.name);
            idEnemyPrefabPairs.Add((EnumManager.ENEMY_TYPE)prefab.GetComponent<CharacterBase>().CharacterId, prefab);
        }
    }

    /// <summary>
    /// 敵のスポーン可能範囲判定処理
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    public (Vector3 minRange, Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < stageMin.position.y)
        {
            minRange.y = stageMin.position.y;
        }

        if (minPoint.x < stageMin.position.x)
        {
            minRange.x = stageMin.position.x;
        }

        if (maxPoint.y > stageMax.position.y)
        {
            maxRange.y = stageMax.position.y;
        }

        if (maxPoint.x > stageMax.position.x)
        {
            maxRange.x = stageMax.position.x;
        }

        return (minRange, maxRange);
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
            if (pos != null)
            {
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
    public void GenerateEnemy(int num)
    {
        // マスタクライアント以外は処理をしない
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        List<SpawnEnemyData> spawnEnemyDatas = new List<SpawnEnemyData>();
        for (int i = 0; i < num; i++)
        {
            // 生成する敵の抽選
            var emitResult = EmitEnemy(emitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("生成する敵の抽選結果がnullのため、以降の処理をスキップします。");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;
            //int listNum = Choose(enemyWeights);   // 元のコード

            EnemyBase enemyBase = idEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            Vector2 spawnRight = (Vector2)player.transform.position + Vector2.right * spawnRangeOffset;
            Vector2 spawnLeft = (Vector2)player.transform.position + Vector2.left * spawnRangeOffset;

            Vector2 minSpawnRight = spawnRight - spawnRight / 2;
            Vector2 maxSpawnRight = spawnRight + spawnRight / 2;

            Vector2 minSpawnLeft = spawnLeft - spawnLeft / 2;
            Vector2 maxSpawnLeft = spawnLeft + spawnLeft / 2;

            // ランダムな位置を生成
            var spawnPostions = CreateEnemySpawnPosition(minSpawnRight, maxSpawnRight);

            //Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

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
                //var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                //var enemyType = (EnumManager.ENEMY_TYPE)listNum;
                //Vector3 scale = idEnemyPrefabPairs[enemyType].transform.localScale;
                //var spawnData = CreateSpawnEnemyData(enemyType, (Vector3)spawnPos, scale, spawnType);
                //SpawnEnemyRequest(spawnData);

                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // 一旦このまま
                spawnEnemyDatas.Add(CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType));
            }

            //Vector3? spawnPos = GenerateEnemySpawnPosition(minSpawnRight, maxSpawnRight,enemyBase);
            //Vector3? spawnPos = GenerateEnemySpawnPosition(minSpawnLeft, maxSpawnLeft, enemyBase);
        }
        SpawnEnemyRequest(spawnEnemyDatas.ToArray());
    }

    /// <summary>
    /// 端末操作時の敵生成処理
    /// </summary>
    public void TerminalGenerateEnemy(int num,Vector2 minPos,Vector2 maxPos)
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

            int listNum = Random.Range(0, idEnemyPrefabPairs.Count);

            EnemyBase enemyBase = idEnemyPrefabPairs[(EnumManager.ENEMY_TYPE)listNum].GetComponent<EnemyBase>();

            // ランダムな位置を生成
            var spawnPostions = CreateEnemyTerminalSpawnPosition(minPos, maxPos);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {

                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // 一旦このまま
                var spawnData = CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);
                
                SpawnEnemyRequest(spawnData);

                // 端末から出た敵をリストに追加
                CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByTerminal);

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

    ///// <summary>
    ///// 敵生成リクエスト
    ///// </summary>
    ///// <param name="spawnEnemy"></param>
    ///// <param name="spawnPos"></param>
    //public GameObject SpawnEnemyRequest(GameObject spawnEnemy, Vector3 spawnPos, bool canPromoteToElite = true)
    //{
    //    GameManager.Instance.SpawnCnt++;

    //    // 生成
    //    GameObject Enemy = Instantiate(spawnEnemy, spawnPos, Quaternion.identity);
    //    if (!canPromoteToElite) return Enemy;

    //    int number = Random.Range(0, 100);

    //    // エリート敵生成限度を設定
    //    int onePercentOfMaxEnemies =
    //        Mathf.FloorToInt(GameManager.Instance.MaxSpawnCnt * 0.3f);

    //    if (number < 5 * ((int)LevelManager.Instance.GameLevel + 1)
    //        && eliteEnemyCnt < onePercentOfMaxEnemies)
    //    {// 5%(* 現在のゲームレベル)以下で、エリート敵生成限度に達していなかったら
    //        // エリート敵生成
    //        Enemy.GetComponent<EnemyBase>().PromoteToElite((EnemyElite.EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4));
    //        eliteEnemyCnt++;
    //    }

    //    return Enemy;
    //}

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
        enemies.Sort((a, b) => a.CharacterId.CompareTo(b.CharacterId));

        EnumManager.ENEMY_TYPE? entryType = null;
        int emitRnd = Random.Range(1, tatalWeight + 1);
        int currentWeight = 0;
        foreach(EnemyBase enemy in enemies)
        {
            currentWeight += enemy.SpawnWeight;
            if (emitRnd <= currentWeight)
            {
                entryType = (EnumManager.ENEMY_TYPE)enemy.CharacterId;
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
        GameManager.Instance.SpawnCnt++;
        ENEMY_ELITE_TYPE eliteType = ENEMY_ELITE_TYPE.None;

        if (canPromoteToElite)
        {
            eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);
        }

        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            EnemyId = GameManager.Instance.SpawnCnt,
            Position = entryData.Position,
            Scale = entryData.Scale,
            SpawnType = spawnType,
            EliteType = eliteType,
        };
    }

    /// <summary>
    /// 敵の生成実行
    /// </summary>
    /// <param name="spawnEnemyData"></param>
    /// <returns></returns>
    void SpawnEnemy(SpawnEnemyData spawnEnemyData)
    {
        if (spawnEnemyData == null)
        {
            Debug.LogWarning("nullの要素が見つかったため、敵の生成を中断しました。");
            return;
        }
        // 敵の生成
        var prefab = idEnemyPrefabPairs[spawnEnemyData.TypeId];
        var position = spawnEnemyData.Position;
        var scale = spawnEnemyData.Scale;
        var eliteType = spawnEnemyData.EliteType;
        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
        enemyObj.transform.localScale = scale;
        enemyObj.GetComponent<EnemyBase>().PromoteToElite(0);
        CharacterManager.Instance.AddEnemies(new SpawnedEnemy(spawnEnemyData.EnemyId, enemyObj, enemyObj.GetComponent<EnemyBase>(), spawnEnemyData.SpawnType));
    }

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

        // ここで敵の生成リクエスト
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // SpawnEnemyAsyncを呼び出す
            await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
        }

        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            SpawnEnemy(spawnEnemyData);
        }
    }

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

    private void OnDrawGizmos()
    {
        if (CharacterManager.Instance && CharacterManager.Instance.PlayerObjSelf)
        {
            var player = CharacterManager.Instance.PlayerObjSelf;
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // 右
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // 左
        }
    }
}