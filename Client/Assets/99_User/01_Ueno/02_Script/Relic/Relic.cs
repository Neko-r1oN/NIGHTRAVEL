using NUnit.Framework;
using UnityEngine;

public class Relic : MonoBehaviour
{
    [SerializeField] int id;

    RelicDeta relicDeta;

    private void Start()
    {
        relicDeta = new RelicDeta(id);
    }

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
    /// </summary>
    public void AddRelic()
    {
        RelicManager.Instance.AddRelic(relicDeta);
    }

    /// <summary>
    /// �v���C���[�Ƃ̐ڐG����
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            AddRelic();
            //gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
