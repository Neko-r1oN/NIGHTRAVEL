//===================
// 端末スクリプト
// Author:Nishiura
// Date:2025/07/01
//===================
using DG.Tweening;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Terminal : MonoBehaviour
{
    // プレイヤーが端末に触れているかの判定変数
    private bool isPlayerIn = false;
    // 使用判定
    private bool isUsed = false;
    // 端末の種別
    public int terminalType;

    //生成する敵の最大数
    [SerializeField] int maxSpawnEnemy;

    public int TerminalType { get { return terminalType; } }

    protected bool isBoot = false;    // true：ON, false：OFF
    public bool IsBoot { get { return isBoot; } set { isBoot = value; } }

    //UIManager
    UIManager uiManager;
    public GameObject TerminalObj {  get; private set; }

    //TimerDirecter
    TimerDirector timerDirector;

    //制限時間
    public int limitTime;

    bool isTerminal;

    //端末のアイコン
    [SerializeField] GameObject terminalIcon;

    // スピード用ゴールポイントオブジェクトのリスト
    [SerializeField] List<GameObject> pointList;
    [SerializeField] List<Transform> relicSpawnPoints = new List<Transform>();

    List<GameObject> terminalSpawnList = new List<GameObject>();
    public List<GameObject> TerminalSpawnList { get { return terminalSpawnList; } set { terminalSpawnList = value; } }
    List<GameObject> terminalEnemyList = new List<GameObject>();

    GameManager gameManager;
    SpawnManager spawnManager;
    PlayerBase player;

    //レリック管理クラス
    RelicManager relicManager;

    private static Terminal instance;

    public static Terminal Instance
    {
        get { return instance; }
    }

    // 端末タイプ列挙型
    public enum TerminalCode
    {
        Type_Enemy = 1,
        Type_Speed,
        Type_Deal,
        Type_Jumble,
        Type_Elite,
        Type_Boss
    }

    public TerminalCode code;

    public Dictionary<TerminalCode, string> Terminalexplanation = new Dictionary<TerminalCode, string>
    {
        {TerminalCode.Type_Enemy,"出現した敵を全て倒せ" },
        {TerminalCode.Type_Speed,"出現したゲートを全て通れ" },
        {TerminalCode.Type_Deal,"取引成立" },
        {TerminalCode.Type_Jumble,"" },
        {TerminalCode.Type_Elite,"出現したエリート敵を全て倒せ" },
        {TerminalCode.Type_Boss,"" }
    };

    


    public bool IsTerminal { get { return isTerminal; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = UIManager.Instance;
        spawnManager = SpawnManager.Instance;
        isTerminal = false;
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
            player = collision.gameObject.GetComponent<PlayerBase>();

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
    /// ギミックの起動
    /// </summary>
    public virtual void TurnOnPower()
    {
        isBoot = true;
    }

    /// <summary>
    /// ギミック起動リクエスト
    /// </summary>
    /// <param name="player">起動したプレイヤー</param>
    public void TurnOnPowerRequest(GameObject player)
    {
        // オフライン用
        if (!RoomModel.Instance)
        {
            TurnOnPower();
        }
        // マルチプレイ中 && 起動した人が自分自身の場合
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // サーバーに対してリクエスト処理
        }
    }

    /// <summary>
    /// 端末起動処理
    /// </summary>
    private void BootTerminal()
    {
        System.Random rand = new System.Random();
        int rndNum; //敵生成数

        //UIManager.Instance.DisplayTerminalExplanation();

        // 端末タイプで処理を分ける
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // 敵生成の場合
                isUsed = true;  // 使用済みにする
                isTerminal = true;

                rndNum = rand.Next(1,maxSpawnEnemy); // 生成数を乱数(6-10)で設定

                int childrenCnt = this.gameObject.transform.childCount;

                List<Transform> children = new List<Transform>();

                //端末のアイコンを1.5秒かけてフェードアウトする
                //terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                for (int i = 0; i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // 敵生成


                break;

            case (int)TerminalCode.Type_Speed:
                // スピードの場合
                isTerminal = true;
                isUsed = true;  // 使用済みにする
                foreach (var point in pointList)
                {   // 各ゴールポイントを表示
                    point.SetActive(true);
                }

                //端末のアイコンを1.5秒かけてフェードアウトする
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                //カウントダウンする
                InvokeRepeating("CountDown", 1, 1);

                break;

            case (int)TerminalCode.Type_Deal:
                // 取引の場合

                //端末のアイコンを1.5秒かけてフェードアウトする
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                isUsed = true;  // 使用済みにする
                rndNum = rand.Next(0, 6); // 生成数を乱数(0-5)で設定

                //ダメージを与える
                if (terminalType == 3 && isUsed == true)
                {
                    DealDamage();
                }

                break;

            case (int)TerminalCode.Type_Jumble:
                // ごちゃまぜの場合
                isUsed = true;  // 使用済みにする

                //端末のアイコンを1.5秒かけてフェードアウトする
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                JumbleRelic();

                break;

            case (int)TerminalCode.Type_Elite:
                // エリート敵生成の場合
                // 敵生成の場合
                isUsed = true;  // 使用済みにする
                isTerminal = true;

                rndNum = rand.Next(6, maxSpawnEnemy); // 生成数を乱数(6-10)で設定

                childrenCnt = this.gameObject.transform.childCount;

                children = new List<Transform>();

                //端末のアイコンを1.5秒かけてフェードアウトする
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                for (int i = 0; i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // 敵生成

                isTerminal = true;
                break;

            case (int)TerminalCode.Type_Boss:
                if (SpawnManager.Instance.CrashNum >= SpawnManager.Instance.KnockTermsNum)
                {
                    isUsed = true;

                    SpawnManager.Instance.SpawnBoss();
                }

                break;
        }
    }

    [ContextMenu("GiveRewardRequest")]
    private void GiveRewardRequest()
    {
        Stack<Vector2> posStack = new Stack<Vector2>();

        foreach (var point in relicSpawnPoints)
        {
            posStack.Push(point.position);
        }

        //レリックを排出する
        RelicManager.Instance.DropRelicRequest(posStack, false);
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
                //通常敵とのバトルの場合
                // ターミナルの効果を終了する
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Speed:
                // スピードの場合

                // ターミナルの効果を終了する
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                //カウントダウンを停止する
                CancelInvoke("CountDown");

                //報酬を排出
                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Deal:
                // 取引の場合

                // ターミナルの効果を終了する
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Jumble:
                // ごちゃまぜの場合
                isUsed = true;  // 使用済みにする

                GiveRewardRequest();

                // ターミナルの効果を終了する
                isTerminal = false;

                break;
            case (int)TerminalCode.Type_Elite:
                // エリート敵生成の場合
                isUsed = true;

                // ターミナルの効果を終了する
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Boss:
                isUsed = true;

                // ターミナルの効果を終了する
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
        }
    }

    /// <summary>
    /// カウントダウン処理
    /// </summary>
    public void CountDown()
    {
        //limitTImeを1ずつ減らす
        limitTime--;

        var span = new TimeSpan(0, 0, (int)limitTime);
        TimerDirector.Instance.Timer.text = span.ToString(@"mm\:ss");

        //制限時間をcowntDownTextに反映する
        //countDownText.text=limitTime.ToString();

        //制限時間が0以下になったら(時間切れ)
        if (limitTime <= 0)
        {
            //isTerminalをfalseにする
            isTerminal = false;

            //limitTimeを0にする
            limitTime = 0;

            //カウントダウンを停止する
            CancelInvoke("CountDown");


            //ゴールポイントを削除する
            foreach (GameObject obj in pointList)
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// 敵生成処理
    /// </summary>
    /// <param name="num"></param>
    /// <param name="minPos">生成最小位置</param>
    /// <param name="maxPos">生成最大位置</param>
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
                if (code == TerminalCode.Type_Elite)
                {
                    var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                    Vector3 scale = Vector3.one;    // 一旦このまま
                    // var spawnData = spawnManager.CreateTerminalSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                    //spawnManager.SpawnTerminalEnemyRequest(this, spawnData);
                }
                else
                {
                    var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                    Vector3 scale = Vector3.one;    // 一旦このまま
                    var spawnData = spawnManager.CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                    //spawnManager.SpawnTerminalEnemyRequest(this, spawnData);
                }
            }

            enemyCnt++;
        }
    }

    /// <summary>
    /// 取引端末でHPを減らす処理
    /// </summary>
    public void DealDamage()
    {
        int HP = player.HP;

        //減らす量は現在のHPの50%
        int damege = Mathf.FloorToInt(HP * 0.5f);

        //dealDamageが0より小さいか0だったら
        if (damege <= 0)
        {
            //dealDamageを1にする
            damege = 1;
        }

        //HPを減らす
        player.ApplyDamage(damege);

        //レリックを排出する
        GiveReward();
    }

    public void JumbleRelic()
    {
        GiveReward();
        //RelicManager.Instance.ShuffleRelic();
    }

    
}
