using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Relic : Item
{
    RelicData relicData;
    public RelicData RelicData { get { return relicData; } set { relicData = value; } }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    private void AddRelic()
    {
        RelicManager.Instance.AddRelic(relicData);
    }

    public override void OnGetItem(bool isSelfAcquired)
    {
        if (isSelfAcquired) AddRelic();
        base.OnGetItem(isSelfAcquired);
    }

    /// <summary>
    /// プレイヤーとの接触処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            // レリック取得通知
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), collision.gameObject);
        }
    }
}
