using NUnit.Framework;
using UnityEngine;

public class Relic : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int rarity;

    RelicDeta relicDeta;

    public int Rarity { get { return rarity; } }

    private void Start()
    {
        relicDeta = new RelicDeta(id,rarity);
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic()
    {
        relicDeta = new RelicDeta(id, rarity);

        RelicManager.Instance.AddRelic(relicDeta);
    }

    /// <summary>
    /// プレイヤーとの接触処理
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
