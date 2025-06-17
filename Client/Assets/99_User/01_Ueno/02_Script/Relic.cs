using UnityEngine;

public class Relic : MonoBehaviour
{
    [SerializeField] int id;

    public int ID {  get { return id; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic()
    {
        RelicManager.Instance.AddRelic(this);
    }

    /// <summary>
    /// プレイヤーとの接触処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            AddRelic();
            Destroy(this.gameObject);
        }
    }
}
