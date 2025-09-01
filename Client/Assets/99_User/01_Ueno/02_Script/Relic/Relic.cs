using UnityEngine;

public class Relic : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int rarity;

    RelicDeta relicDeta;

    public int Rarity { get { return rarity; } }

    private void Start()
    {
        // レリック情報の取得(サーバー)

        // レリック情報の登録
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
        // レリック取得通知

        if (collision.gameObject.tag == "Player")
        {
            AddRelic();
            //Object.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
