using UnityEngine;

public class Relic : Item 
{
    [SerializeField] int id;
    [SerializeField] int rarity;

    RelicData relicData;

    public int Rarity { get { return rarity; } }

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
