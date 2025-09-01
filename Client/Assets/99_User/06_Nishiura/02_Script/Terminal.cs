//===================
// 端末スクリプト
// Author:Nishiura
// Date:2025/07/01
//===================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
public class Terminal : MonoBehaviour
{
    // プレイヤーが端末に触れているかの判定変数
    private bool isPlayerIn = false;
    // 使用判定
    private bool isUsed = false;
    // 端末の種別
    public int terminalType;

    [SerializeField] int maxSpawnEnemy;

    public int TerminalType { get { return terminalType; } }

    // スピード用ゴールポイントオブジェクトのリスト
    [SerializeField] List<GameObject> pointList;

    List<GameObject> terminalSpawnList = new List<GameObject>();
    public List<GameObject> TerminalSpawnList { get {  return terminalSpawnList; } set { terminalSpawnList = value; } }
    //List<GameObject> terminalEnemyList = new List<GameObject>();

    GameManager gameManager;
    SpawnManager spawnManager;

    private static Terminal instance;

    public static Terminal Instance
    {
        get { return instance; }
    }

    // 端末タイプ列挙型
    public enum TerminalCode 
    {
        None = 0,
        Type_Enemy,
        Type_Speed,
        Type_Deal,
        Type_Recycle,
        Type_Jumble,
        Type_Return,
        Type_Elite
    }

    public TerminalCode code;

    public Dictionary<TerminalCode, string> Terminalexplanation = new Dictionary<TerminalCode, string>
    {
        {TerminalCode.None,""},
        {TerminalCode.Type_Enemy,"出現した敵を全て倒せ" },
        {TerminalCode.Type_Speed,"出現したゲートを全て通れ" },
        {TerminalCode.Type_Deal,"" },
        {TerminalCode.Type_Recycle,""},
        {TerminalCode.Type_Jumble,"" },
        {TerminalCode.Type_Return,"" },
        {TerminalCode.Type_Elite,"" }
    };

    bool isTerminal;

    public bool IsTerminal {  get { return isTerminal; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        //else
        //{
        //    // インスタンスが複数存在しないように、既に存在していたら自身を消去する
        //    Destroy(gameObject);
        //}
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isTerminal = false;
        spawnManager = SpawnManager.Instance;
    }

    private void Update()
    {
        // Eキー入力かつプレイヤーが端末に触れている場合かつその端末が未使用である場合、端末を起動
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // 端末を起動
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーが端末付近に接近した場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = true;  // 触れたこととする
            Debug.Log("You Touched Terminal");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーが端末から離れた場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = false; // 触れていないこととする
            Debug.Log("No Terminal");
        }
    }

    /// <summary>
    /// 端末起動処理
    /// </summary>
    private void BootTerminal()
    {     
        System.Random rand = new System.Random();
        int rndNum;

        UIManager.Instance.DisplayTerminalExplanation();

        // 端末タイプで処理を分ける
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // 敵生成の場合
                isUsed = true;  // 使用済みにする
                isTerminal = true;

                rndNum = rand.Next(1, maxSpawnEnemy); // 生成数を乱数(6-10)で設定

                int childrenCnt = this.gameObject.transform.childCount;

                List<Transform> children = new List<Transform>();

                for ( int i = 0;i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // 敵生成
                isTerminal = true;
                break;

            case (int)TerminalCode.Type_Speed:
                // スピードの場合
                isTerminal = true;
                isUsed = true;  // 使用済みにする
                foreach (var point in pointList)
                {   // 各ゴールポイントを表示
                    point.SetActive(true);
                }
                Invoke("TimeUp",10f);   // 10秒後タイムアップとする
                break;

            case (int)TerminalCode.Type_Deal:
                // 取引の場合
                isUsed = true;  // 使用済みにする
                rndNum = rand.Next(0, 6); // 生成数を乱数(0-5)で設定

                break;

            case (int)TerminalCode.Type_Jumble:
                // ごちゃまぜの場合
                isUsed = true;  // 使用済みにする

                break;

            case (int)TerminalCode.Type_Elite:
                // エリート敵生成の場合
                isUsed = true;  // 使用済みにする

                break;

            case (int)TerminalCode.Type_Recycle:
                // リサイクルの場合
                isUsed = true;  // 使用済みにする

                break;

            case (int)TerminalCode.Type_Return:
                // 再帰の場合
                isUsed = true;  // 使用済みにする

                break;
        }
    }

    /// <summary>
    /// 報酬排出処理
    /// </summary>
    public void GiveReward()
    {
        // 端末タイプで処理を分ける
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // 敵生成の場合
                isUsed = true;
                break;
            case (int)TerminalCode.Type_Speed:
                // スピードの場合
                Debug.Log("OMFG Reward Here!!!!!");
                break;
            case (int)TerminalCode.Type_Deal:
                // 取引の場合
                isUsed = true;  // 使用済みにする

                break;
            case (int)TerminalCode.Type_Jumble:
                // ごちゃまぜの場合
                isUsed = true;  // 使用済みにする

                break;
            case (int)TerminalCode.Type_Elite:
                // エリート敵生成の場合
                isUsed = true;  // 使用済みにする

                break;
            case (int)TerminalCode.Type_Recycle:
                // リサイクルの場合
                isUsed = true;  // 使用済みにする

                break;
            case (int)TerminalCode.Type_Return:
                // 再帰の場合
                isUsed = true;  // 使用済みにする

                break;
        }
    }

    /// <summary>
    /// ゴールポイントに触れた際の処理
    /// </summary>
    /// <param name="obj"></param>
    public void HitGoalPoint(GameObject obj)
    {
        if (pointList.Contains(obj))    // 渡されたオブジェクトがリスト内にあった場合
        {
            pointList.Remove(obj);  // それを除去する
            Destroy(obj);   // それを破壊する

            if(pointList.Count <= 0)
            { // リストが空になった場合、報酬を付与する
                GiveReward();
            }
        }
    }

    /// <summary>
    /// 時間切れ処理
    /// </summary>
    private void TimeUp()
    {
        foreach (var point in pointList)
        {   // 各ゴールポイントを表示
        
            point.SetActive(false);
        }
    }

    private void TerminalGenerateEnemy(int num, Vector2 minPos, Vector2 maxPos)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
        {
            // 生成する敵の抽選
            var emitResult = spawnManager.EmitEnemy(spawnManager.EmitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("生成する敵の抽選結果がnullのため、以降の処理をスキップします。");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;

            EnemyBase enemyBase = spawnManager.IdEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            // ランダムな位置を生成
            var spawnPostions = spawnManager.CreateEnemyTerminalSpawnPosition(minPos, maxPos);

            Vector3? spawnPos = spawnManager.GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                Vector3 scale = Vector3.one;    // 一旦このまま
                var spawnData = spawnManager.CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                spawnManager.SpawnTerminalEnemyRequest(this,spawnData);
            }

            enemyCnt++;
        }   
    }


}
