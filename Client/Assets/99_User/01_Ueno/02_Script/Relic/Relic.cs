using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Relic : Item 
{
    [SerializeField] string id;
    [SerializeField] RARITY_TYPE rarity;

    

    RelicData relicData;

    public RARITY_TYPE Rarity { get { return rarity; } }

    private void Start()
    {
        // レリック情報の取得(サーバー)

        // レリック情報の登録
        relicData = new RelicData(id,rarity);
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic()
    {
        relicData = new RelicData(id, rarity);

        RelicManager.Instance.AddRelic(relicData);

        OnGetItem();
    }

    /// <summary>
    /// プレイヤーとの接触処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // レリック取得通知

        if (collision.gameObject.tag == "Player")
        {
            AddRelic();
            //Object.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
