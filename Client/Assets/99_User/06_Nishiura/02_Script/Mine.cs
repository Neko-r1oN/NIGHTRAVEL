//===================
// 地雷スクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] GameObject boomEffect; // 爆発エフェクトプレハブ

    Vector2 pos;

    private void Start()
    {
        // このゲームオブジェクトのポジションを取得
        pos = this.gameObject.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player" /*&& collision.gameObject == CharacterManager.Instance.PlayerObjSelf*/)
        {
            Instantiate(boomEffect, pos + new Vector2(0.0f, 0.5f), Quaternion.identity);    // 爆発エフェクトを生成
            Destroy(this.gameObject);   // 自身を破壊
            Debug.Log("Boomed Mine");
        }
        else if (collision.transform.tag == "Enemy" && !RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            Instantiate(boomEffect, pos, Quaternion.identity);    // 爆発エフェクトを生成
            Destroy(this.gameObject);   // 自身を破壊
            Debug.Log("Boomed Mine");
        }
    }
}
