//===================
// エレベータースクリプト
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;

public class Elevator : GimmickBase
{
    // 電源判定
    public bool isPowerd;
    // 上昇判定
    bool isRised = false;
    // 上昇値
    public float risePow;
    // 下降値
    public float descentPow;
    // 昇降速度値
    public int moveSpeed;
    // 動作中判定変数
    public bool isMoving;
    // 破壊判定変数
    public bool isBroken = false;
    // ワイヤー
    [SerializeField] GameObject wire;

    // 上昇ボタン
    [SerializeField] GameObject riseButton;
    // 下降ボタン
    [SerializeField] GameObject descButton;

    Tweener tweener;
    private void Update()
    {
        if (isBroken)
        {
            tweener.Kill();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // 電源offまたはエレベーター動作中の場合処理しない

        // プレイヤーがエレベーター内に入った場合
        if (collision.transform.tag == "Player")
        {
            isMoving = true;    // 動作中にする
            Invoke("MoveElevator", 1f);  //昇降開始
        }
    }

    private void MoveElevator()
    {
        Invoke("MovingCheck", 4f);  //動作チェック
        if (!isRised)
        {   //上昇済みでない場合
            tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //上昇する
            if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //ワイヤー上昇する
        }
        else
        {   //上昇済みの場合
            tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //下降する
            if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //ワイヤーも下降する
           
        }
    }

    /// <summary>
    /// 動作確認関数
    /// </summary>
    private void MovingCheck()
    {
        if (!isRised) 
        {   // 上昇していないと判断された場合
            isRised=true;       // 上昇済みとする
        }
        else
        {   // 上昇していると判断された場合
            isRised = false;    // 下降済みとする
        }

        isMoving = false;   // 動作完了にする
    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        if (isPowerd) return;   // すでに起動してある場合は処理しない
        isPowerd = true;
    }

    /// <summary>
    /// 昇降ボタン処理
    /// </summary>
    public void MoveButton(bool type)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // 電源offまたはエレベーター動作中の場合処理しない

        isMoving = true;    // 動作中にする

        if (type == false)
        {
            Invoke("MovingCheck", 4f);  //動作チェック

            if (isRised)
            {   //上昇済みの場合
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //下降する
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //ワイヤーも下降する
            }
        }
        else
        {
            Invoke("MovingCheck", 4f);  //動作チェック
            if (!isRised)
            {   //上昇済みでない場合
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //上昇する
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //ワイヤー上昇する
            }
        }
    }
}
