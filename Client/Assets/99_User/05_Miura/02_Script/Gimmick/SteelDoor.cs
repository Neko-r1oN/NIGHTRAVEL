//=================
//鋼鉄製自動ドアのスクリプト
//Aouther:y-miura
//Date:20025/06/23
//=================

using DG.Tweening;
using System.Configuration;
using UnityEngine;

public class SteelDoor : GimmickBase
{
    [SerializeField] GameObject doorObj;
    Vector2 initPos = Vector2.zero;//初期位置
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //初期位置を設定
        initPos = transform.position;
    }

    /// <summary>
    /// 鋼鉄製自動ドアを開く処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerStay2D(Collider2D collision)
    {//センサー部分にものが触れたら
        if (collision.CompareTag("Player"))
        {//「Player」タグが付いたものが触れたら
            //ドアを開く
            doorObj.transform.DOMoveY(this.transform.position.y+5, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//「Player」タグが付いたものが離れたら
         //ドアを閉じる
            doorObj.transform.DOMoveY(initPos.y, 0.5f);
        }
    }

    public override void TurnOnPower(int triggerID)
    {
        isPowerd = true;
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }

}