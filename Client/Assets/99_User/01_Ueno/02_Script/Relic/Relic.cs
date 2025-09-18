using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Relic : Item
{
    RelicData relicData;
    public RelicData RelicData { get { return relicData; } set { relicData = value; } }

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
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
    /// �v���C���[�Ƃ̐ڐG����
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            // �����b�N�擾�ʒm
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), collision.gameObject);
        }
    }
}
