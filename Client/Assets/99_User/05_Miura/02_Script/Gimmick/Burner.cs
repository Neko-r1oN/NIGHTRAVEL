//===================
// バーナーに関する処理
// Aouther:y-miura
// Date:2025/07/07
//===================

using System.Collections;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Burner : MonoBehaviour
{
    PlayerBase player;
    EnemyBase enemy;
    [SerializeField] GameObject flame;

    private void Start()
    {

    }

    private void Update()
    {

    }

    /// <summary>
    /// 触れたオブジェクトに炎上効果を付与する処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = GetComponent<PlayerBase>();

            Debug.Log("プレイヤーに炎上状態を付与");
        }

        if (collision.CompareTag("Enemy"))
        {
            enemy = GetComponent<EnemyBase>();

            Debug.Log("敵に炎上状態を付与");
        }
    }

    /// <summary>
    /// 炎を点けたり消したりする処理
    /// </summary>
    private void Ignition()
    {

    }
}
