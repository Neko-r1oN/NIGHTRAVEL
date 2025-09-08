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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
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
    /// �A�C�e���l�����N�G�X�g
    /// </summary>
    /// <param name="item">�A�C�e�����</param>
    /// <param name="player">�l�����悤�Ƃ��Ă���v���C���[</param>
    public async void GetItemRequest(Item item, GameObject player)
    {
        if (RoomModel.Instance)
        {
            // �}���`�v���C�� && ���g���l�������ꍇ
            if(player == CharacterManager.Instance.PlayerObjSelf)
            {
                await RoomModel.Instance.GetItemAsync(item.ItemType, item.UniqueId);
            }
        }
        else
        {
            // �I�t���C���p
            managedItems[item.UniqueId].OnGetItem(true);
        }
    }

    /// <summary>
    /// �A�C�e���l���ʒm
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
    /// �A�C�e�����X�g�ɒǉ�
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
