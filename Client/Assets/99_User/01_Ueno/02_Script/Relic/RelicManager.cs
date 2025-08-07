//----------------------------------------------------
// レリック管理クラス
// Author : Souma Ueno
//----------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class RelicManager : MonoBehaviour
{
    #region リスト
    [Header("リスト")]
    [SerializeField] List<Sprite> relicSprites = new List<Sprite>();     // レリックのリスト
    [SerializeField] List<RelicDeta> haveRelicList = new List<RelicDeta>();     // 所持レリックリスト
    [SerializeField] List<GameObject> relicPrefab = new List<GameObject>();  // レリックプレファブ
    #endregion

    float elapsedTime;
    GameObject relic;

    RELIC_RARITY randomRarity;

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

    private void Start()
    {
        if (RoomModel.Instance == null) return;
        //モデルでOnSpawnedRelicが呼び出されるとOnSpawnRelicが呼び出される
        //RoomModel.Instance.OnSpawnedRelic += this.OnSpawnRelic;
    }

    /// <summary>
    /// レアリティの確率の設定
    /// </summary>
    private Dictionary<RELIC_RARITY, float> rarityWeight = new Dictionary<RELIC_RARITY, float>()
    {   
        {RELIC_RARITY.CURSE,3 },    // 確率：3%
        {RELIC_RARITY.NORMAL,70 },  // 確率：70%
        {RELIC_RARITY.RARE,20 },    // 確率：20%
        {RELIC_RARITY.SPECIAL,6 },  // 確率：6%
        {RELIC_RARITY.UNIQUE,1 }    // 確率：1%
    };

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
        //Debug.Log(haveRelicList[haveRelicList.Count - 1]);
        Debug.Log(randomRarity);
    }

    /// <summary>
    /// レリックを生成する処理
    /// </summary>
    //public void GenerateRelic(Vector3 bossPos)
    //{
    //    randomRarity = GetRandomRarity();

    //    List<GameObject> filteredRelics = relicPrefab.
    //        Where(prefab =>
    //        {
    //            Relic relic = prefab.GetComponent<Relic>();
    //            return relic != null && relic.Rarity == (int)randomRarity;
    //        }).ToList();

    //    if (filteredRelics.Count > 0)
    //    {
    //        int random = Random.Range(0, filteredRelics.Count);
    //        GameObject selectedRelic = filteredRelics[random];
    //        relic = Instantiate(selectedRelic, bossPos, Quaternion.identity);
    //    }

    //    if (relic != null)
    //    {
    //        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbodyを取得
    //        float boundRnd = Random.Range(2f, 6f);
    //        boundRnd = (int)Random.Range(0f, 2f) == 0 ? boundRnd : boundRnd * -1;
    //        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);    // 力を設定
    //        rb.AddForce(force, ForceMode2D.Impulse);             // 力を加える
    //    }
    //}


    [ContextMenu("GenerateRelicTest")]
    public async void GenerateRelicTest()
    {
        randomRarity = GetRandomRarity();

        List<GameObject> filteredRelics = relicPrefab.
            Where(prefab =>
            {
                Relic relic = prefab.GetComponent<Relic>();
                return relic != null && relic.Rarity == (int)randomRarity;
            }).ToList();

        if (filteredRelics.Count > 0)
        {
            int random = Random.Range(0, filteredRelics.Count);
            GameObject selectedRelic = filteredRelics[random];
#if UNITY_EDITOR
            relic = Instantiate(selectedRelic, new Vector3(0, 0, 0), Quaternion.identity);
#else
            //レリックの設定
            relic = selectedRelic;
            //レリックの生成同期を実行
            await RoomModel.Instance.SpawnRelicAsync(relic.transform.position);
#endif
        }
    }

    /// <summary>
    /// レリックの生成の通知
    /// </summary>
    /// <param name="relicId"></param>
    /// <param name="pos"></param>
    void OnSpawnRelic(int relicId,Vector2 pos)
    {
       relic = Instantiate(relicPrefab[relicId], pos,Quaternion.identity);
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

    /// <summary>
    /// 同じレリックを持っている数を数える
    /// </summary>
    /// <param name="id"></param>
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

    /// <summary>
    /// レアリティ抽出
    /// </summary>
    /// <returns></returns>
    public RELIC_RARITY GetRandomRarity()
    {
        float totalWeight = 0;
        float currentWeight = 0;

        foreach(var pair in rarityWeight)
        {
            totalWeight += pair.Value;
        }

        float randomNum = Random.Range(0, totalWeight);

        foreach(var pair in rarityWeight)
        {
            currentWeight += pair.Value;
            if(randomNum < currentWeight)
            {
                return pair.Key;
            }
        }

        return RELIC_RARITY.NORMAL;
    }
}