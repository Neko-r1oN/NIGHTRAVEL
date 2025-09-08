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
        // �����b�N���̎擾(�T�[�o�[)

        // �����b�N���̓o�^
        relicData = new RelicData(id,rarity);
    }

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
    /// </summary>
    public void AddRelic()
    {
        relicData = new RelicData(id, rarity);

        RelicManager.Instance.AddRelic(relicData);

        OnGetItem();
    }

    /// <summary>
    /// �v���C���[�Ƃ̐ڐG����
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �����b�N�擾�ʒm

        if (collision.gameObject.tag == "Player")
        {
            AddRelic();
            //Object.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
