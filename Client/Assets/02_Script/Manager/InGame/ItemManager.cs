using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] List<Item> items = new List<Item>();
    Dictionary<string, Item> managedItems = new Dictionary<string, Item>();

    static ItemManager instance;
    public static ItemManager Instance
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
            for (int i = 0; i < items.Count; i++)
            {
                items[i].UniqueId = i + "-" + items[i].name;
                managedItems.Add(items[i].UniqueId, items[i]);
            }
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnGetItemSyn += OnGetItem;

    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnGetItemSyn -= OnGetItem;
    }

    public int GetItemListCount()
    {
        return managedItems.Count;
    }

    /// <summary>
    /// アイテム獲得リクエスト
    /// </summary>
    /// <param name="item">アイテム情報</param>
    /// <param name="player">獲得しようとしているプレイヤー</param>
    public async void GetItemRequest(Item item, GameObject player)
    {
        if (RoomModel.Instance)
        {
            // マルチプレイ中 && 自身が獲得した場合
            if(player == CharacterManager.Instance.PlayerObjSelf)
            {
                await RoomModel.Instance.GetItemAsync(item.ItemType, item.UniqueId);
            }
        }
        else
        {
            // オフライン用
            managedItems[item.UniqueId].OnGetItem(true);
        }
    }

    /// <summary>
    /// アイテム獲得通知
    /// </summary>
    public void OnGetItem(Guid conId, string itemID)
    {
        if (managedItems.ContainsKey(itemID))
        {
            bool isSelfAcquired = true;
            if(RoomModel.Instance) isSelfAcquired = RoomModel.Instance.ConnectionId == conId;
            managedItems[itemID].OnGetItem(isSelfAcquired);
            managedItems.Remove(itemID);
        }
    }

    /// <summary>
    /// アイテムリストに追加
    /// </summary>
    /// <param name="relicDatas"></param>
    public void AddItemFromList(string uniqueId, Item item)
    {
        if (!managedItems.ContainsKey(uniqueId))
        {
            managedItems.Add(uniqueId, item);
        }
    }
}
