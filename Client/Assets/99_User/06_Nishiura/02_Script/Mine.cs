//===================
// 地雷スクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;

public class Mine : GimmickBase
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
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
        }
        else if (collision.transform.tag == "Enemy")
        {
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
            }
        }
    }

    public override void TurnOnPower()
    {
        GetComponent<AudioSource>().Play();

        Instantiate(boomEffect, pos, Quaternion.identity);    // 爆発エフェクトを生成
        Destroy(this.gameObject);   // 自身を破壊
    }
}
