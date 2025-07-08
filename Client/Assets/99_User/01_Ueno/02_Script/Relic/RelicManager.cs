//----------------------------------------------------
// レリック管理クラス
// Author : Souma Ueno
//----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RelicManager : MonoBehaviour
{
    #region リスト
    [Header("リスト")]
    [SerializeField] List<Sprite> relicSprites = new List<Sprite>();     // レリックのリスト
    [SerializeField] List<RelicDeta> haveRelicList = new List<RelicDeta>();     // 所持レリックリスト
    [SerializeField] List<GameObject> relicPrefab = new List<GameObject>();  // レリックプレファブ
    #endregion

    float elapsedTime;

    /// <summary>
    /// レリックのレアリティ
    /// </summary>
    public enum RELIC_RARITY
    {
        CURSE,　// 呪い
        NORMAL, // ノーマル
        RARE,   // レア
        UNIQUE, // ユニーク
        SPECIAL // 特殊
    }

    private static RelicManager instance;

    public static RelicManager Instance
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
            // インスタンスが複数存在しないように、
            // 既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic(RelicDeta relic)
    {       
        if (haveRelicList.Find(X => X.ID == relic.ID) != null)
        {
            CountRelic(relic.ID);
        }
        else
        {
            UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
        }

        haveRelicList.Add(relic);
        Debug.Log(haveRelicList[haveRelicList.Count - 1]);

        /*foreach(Relic rlc in haveRelicList)
        {
            if (haveRelicList.Count > 1)
            {
                if (relic.ID == rlc.ID)
                {
                    CountRelic(relic.ID);
                }
                else
                {
                    UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
                }
            }
        }*/
    }

    /// <summary>
    /// レリックを生成する処理
    /// </summary>
    public void GenerateRelic(Vector3 bossPos)
    {
        int rand = Random.Range(0, relicSprites.Count);

        GameObject relic = Instantiate(relicPrefab[rand],bossPos,Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbodyを取得
        float boundRnd = Random.Range(2f, 6f);
        boundRnd = (int)Random.Range(0f, 2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // 力を設定
        rb.AddForce(force, ForceMode2D.Impulse);          // 力を加える
    }

    [ContextMenu("GenerateRelicTest")]
    public void GenerateRelicTest()
    {
        int rand = Random.Range(0, relicSprites.Count);

        GameObject relic = Instantiate(relicPrefab[rand], new Vector3(0,0,0), Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbodyを取得
        float boundRnd = Random.Range(2f,6f);
        boundRnd = (int)Random.Range(0f,2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // 力を設定
        rb.AddForce(force, ForceMode2D.Impulse);          // 力を加える
    }

    [ContextMenu("ShuffleRelic")]
    /// <summary>
    /// 持ち物を入れ替える処理
    /// </summary>
    public void ShuffleRelic()
    {
        int count = haveRelicList.Count;
        haveRelicList.Clear();
        UIManager.Instance.ClearRelic();
        for (int i = 0; i < count; i++)
        {
            int relicnum = Random.Range(0, relicSprites.Count);
            relicPrefab[relicnum].GetComponent<Relic>().AddRelic();
        }
    }

    public void CountRelic(int id)
    {
        int relicCnt = 0;

        foreach (RelicDeta rlc in haveRelicList)
        {
            if (id == rlc.ID)
            {
                relicCnt++;
            }
        }

        UIManager.Instance.totalRelics(relicSprites[id], relicCnt);
    }
}