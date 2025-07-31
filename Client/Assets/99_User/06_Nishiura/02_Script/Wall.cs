using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーがエレベーター内に入った場合
        if (collision.transform.tag == "Player")
        {
            GetComponent<TilemapRenderer>().material.DOFade(0, 1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーがエレベーター内に入った場合
        if (collision.transform.tag == "Player")
        {
            GetComponent<TilemapRenderer>().material.DOFade(1, 1);
        }
    }
}
