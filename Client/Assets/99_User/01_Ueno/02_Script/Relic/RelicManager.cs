//----------------------------------------------------
// レリック管理クラス
// Author : Souma Ueno
//----------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;


public class RelicManager : MonoBehaviour
{
    #region リスト
    [Header("リスト")]
    [SerializeField] List<Sprite> relicSprites = new List<Sprite>();     // レリックのリスト
    public List<Sprite> RelicSprites { get {  return relicSprites; } }
    [SerializeField] List<GameObject> relicSpawnPos = new List<GameObject>();
    [SerializeField] GameObject relicPrefab;
    [SerializeField] List<Material> rarityMaterial = new List<Material>();

    // 現在所持しているレリックのリスト
    static public List<RelicData> HaveRelicList { get; set; } = new List<RelicData>();

    #endregion

    float elapsedTime;
    GameObject relic;

    public Material defaultSpriteMaterial;

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
        ApplyHaveRelicsUI();

        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnDropedRelic += this.OnDropRelic;
        RoomModel.Instance.OnTerminalJumbled += this.OnTerminalJumbled;
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnDropedRelic -= this.OnDropRelic;
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic(RelicData relic)
    {
        if (HaveRelicList.Find(X => X.ID == relic.ID) != null)
        {
            CountRelic(relic.ID);
            UIManager.Instance.GetRelicBanner(relicSprites[(int)relic.ID - 1]);
        }
        else
        {
            UIManager.Instance.DisplayRelic(relicSprites[(int)relic.ID - 1],relic);
        }

        HaveRelicList.Add(relic);
    }

    /// <summary>
    /// 現在所持しているレリックをUIに適用させる
    /// </summary>
    void ApplyHaveRelicsUI()
    {
        foreach(var relic in HaveRelicList)
        {
            UIManager.Instance.DisplayRelic(relicSprites[(int)relic.ID - 1], relic);
        }
    }

    /// <summary>
    /// レリックを生成する処理
    /// </summary>
    public void GenerateRelic(Dictionary<string, DropRelicData> relicDatas)
    {
        foreach (var data in relicDatas)
        {
            relic = Instantiate(relicPrefab, data.Value.SpawnPos, Quaternion.identity);
            relic.GetComponent<Item>().UniqueId = data.Key;

            relic.GetComponent<Relic>().RelicData = 
                new RelicData(data.Value.RelicType,data.Value.RarityType,data.Value.Name,data.Value.ExplanationText);

            SpriteRenderer spriteRenderer = relic.transform.GetChild(0).GetComponent<SpriteRenderer>();
            SpriteRenderer sr = relic.transform.GetChild(0).GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = relicSprites[(int)data.Value.RelicType - 1];
            }

            if (sr != null)
            {
                // ここでマテリアルを割り当て
                sr.material = rarityMaterial[(int)data.Value.RarityType - 1];
            }

            ItemManager.Instance.AddItemFromList(data.Value.uniqueId, relic.GetComponent<Item>());
        }

        // 下記コードレアリティ抽出予定処理　一応残してる
        //randomRarity = GetRandomRarity();

        //List<GameObject> filteredRelics = relicPrefab.
        //    Where(prefab =>
        //    {
        //        Relic relic = prefab.GetComponent<Relic>();
        //        return relic != null && relic.Rarity == (int)randomRarity;
        //    }).ToList();

        //if (filteredRelics.Count > 0)
        //{
        //    int random = Random.Range(0, filteredRelics.Count);
        //    GameObject selectedRelic = filteredRelics[random];
        //    relic = Instantiate(selectedRelic, bossPos, Quaternion.identity);
        //}

        //if (relic != null)
        //{
        //    Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbodyを取得
        //    float boundRnd = Random.Range(2f, 6f);
        //    boundRnd = (int)Random.Range(0f, 2f) == 0 ? boundRnd : boundRnd * -1;
        //    Vector3 force = new Vector3(boundRnd, 12.0f, 0f);    // 力を設定
        //    rb.AddForce(force, ForceMode2D.Impulse);             // 力を加える
        //}
    }

    /// <summary>
    /// レリック入れ替え処理
    /// </summary>
    /// <param name="relics"></param>
    public void OnTerminalJumbled (List<DropRelicData> relics)
    {
        // レリック消去
        HaveRelicList.Clear();
        UIManager.Instance.ClearRelic();

        // 受け取ったレリックデータを適用
        foreach(var relic in relics)
        {
            // レリックデータの作成
            var data = new RelicData(relic.RelicType, relic.RarityType,relic.Name,relic.ExplanationText);

            AddRelic(data);
        }
    }

    /// <summary>
    /// 同じレリックを持っている数を数える
    /// </summary>
    /// <param name="id"></param>
    public void CountRelic(RELIC_TYPE id)
    {
        int relicCnt = 0;

        foreach (RelicData rlc in HaveRelicList)
        {
            if (id == rlc.ID)
            {
                relicCnt++;
            }
        }

        UIManager.Instance.totalRelics(relicSprites[(int)id], relicCnt);
    }

    ///// <summary>
    ///// レアリティ抽出
    ///// </summary>
    ///// <returns></returns>
    //public RELIC_RARITY GetRandomRarity()
    //{
    //    float totalWeight = 0;
    //    float currentWeight = 0;

    //    foreach(var pair in rarityWeight)
    //    {
    //        totalWeight += pair.Value;
    //    }

    //    float randomNum = Random.Range(0, totalWeight);

    //    foreach(var pair in rarityWeight)
    //    {
    //        currentWeight += pair.Value;
    //        if(randomNum < currentWeight)
    //        {
    //            return pair.Key;
    //        }
    //    }

    //    return RELIC_RARITY.NORMAL;
    //}

    /// <summary>
    /// レリックドロップリクエスト
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="includeBossRarity"></param>
    public async void DropRelicRequest(Stack<Vector2> pos , bool includeBossRarity)
    {
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            await RoomModel.Instance.DropRelicAsync(pos, includeBossRarity);
        }
        else if(!RoomModel.Instance)
        {
            DropRelicData dropRelic = new DropRelicData()
            {
                uniqueId = Guid.NewGuid().ToString(),
                Name = "適当",
                ExplanationText = "説明",
                RelicType = (EnumManager.RELIC_TYPE)Random.Range(1, 23),
                RarityType = EnumManager.RARITY_TYPE.Common,
                SpawnPos = pos.Pop()
            };
            Dictionary<string,DropRelicData> dropRelics = new Dictionary<string, DropRelicData> ();
            dropRelics.Add(dropRelic.uniqueId, dropRelic);

            GenerateRelic(dropRelics);
        }
    }

    /// <summary>
    /// レリックのドロップ通知
    /// </summary>
    /// <param name="relicDatas"></param>
    public void OnDropRelic(Dictionary<string, DropRelicData> relicDatas)
    {
        GenerateRelic(relicDatas);
    }

    /// <summary>
    /// レアリティ抽選処理
    /// </summary>
    /// <param name="isBoss"></param>
    /// <returns></returns>
    RARITY_TYPE DrawRarity(bool includeBossRarity)
    {
        // レアリティの種類
        Dictionary<RARITY_TYPE, int> raritys = new Dictionary<RARITY_TYPE, int>()
            {
                { RARITY_TYPE.Legend, 2 },
                { RARITY_TYPE.Unique, 6 },
                { RARITY_TYPE.Rare, 12},
                { RARITY_TYPE.Uncommon, 35},
                { RARITY_TYPE.Common, 45 },
                { RARITY_TYPE.Boss, 45 }
            };

        if (includeBossRarity) raritys.Remove(RARITY_TYPE.Common);
        else raritys.Remove(RARITY_TYPE.Boss);

        RARITY_TYPE result = RARITY_TYPE.Common;
        int totalWeight = raritys.Values.Sum();
        int currentWeight = 0;
        int rndPoint = Random.Range(1, totalWeight);

        foreach (var rarity in raritys)
        {
            currentWeight += rarity.Value;
            if (rndPoint <= currentWeight)
            {
                result = rarity.Key;
                break;
            }
        }
        return result;
    }
}