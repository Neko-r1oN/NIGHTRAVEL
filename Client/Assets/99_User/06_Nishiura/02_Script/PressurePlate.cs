//===================
// 感圧版スクリプト
// Author:Nishiura
// Date:2025/07/01
//===================
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : GimmickBase
{
    // 押下判定変数
    private bool isPushed = false;

    // 関連付けられたギミックリスト
    [SerializeField] List<GimmickBase> linkedGimmick = new List<GimmickBase>();
    [SerializeField] AudioSource pressSE;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーが触れた場合かつ押された状態でない場合
        if (collision.transform.tag == "Player" && isPushed != true && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isPushed = true;
            TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
            pressSE.Play();
        }
    }

    public override void TurnOnPower()
    {
        isPushed = true;
        this.transform.DOMoveY((this.gameObject.transform.position.y - 0.19f), 0.2f);

        foreach (GimmickBase gimmick in linkedGimmick)
        {
            var playerSelf = CharacterManager.Instance.PlayerObjSelf;
            gimmick.TurnOnPowerRequest(playerSelf);
        }
    }
}
