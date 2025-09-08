using UnityEngine;

public class Relic : Item 
{
    [SerializeField] int id;
    [SerializeField] int rarity;

    RelicData relicData;

    public int Rarity { get { return rarity; } }

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
