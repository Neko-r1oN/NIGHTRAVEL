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
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic()
    {
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
