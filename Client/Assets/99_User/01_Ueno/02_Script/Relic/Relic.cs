using UnityEngine;

public class Relic : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int rarity;

    RelicDeta relicDeta;

    public int Rarity { get { return rarity; } }

    private void Start()
    {
        // �����b�N���̎擾(�T�[�o�[)

        // �����b�N���̓o�^
        relicDeta = new RelicDeta(id,rarity);
    }

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
    /// </summary>
    public void AddRelic()
    {
        relicDeta = new RelicDeta(id, rarity);

        RelicManager.Instance.AddRelic(relicDeta);
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
