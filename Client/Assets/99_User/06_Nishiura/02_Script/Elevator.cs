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
    //　エレベーター移動SE
    [SerializeField] AudioSource movementSE;
    //　エレベーター到着SE
    [SerializeField] AudioSource arrivalSE;
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
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isMoving = true;    // 動作中にする
            Invoke("InvokeTurnOnPowerRequest", 0.5f);
        }
    }
    void InvokeTurnOnPowerRequest()
    {
        if (isBroken == true || isPowerd == false) return;
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// 動作確認関数
    /// </summary>
    private void MovingCheck()
    {
        if (!isRised)
        {   // 上昇していないと判断された場合
            //到着SEを再生する
            arrivalSE.Play();
            //移動SEの再生を停止する
            movementSE.Stop();
        }
        else
        {   // 上昇していると判断された場合
            //到着SEを再生する
            arrivalSE.Play();
            //移動SEの再生を停止する
            movementSE.Stop();
        }
        isMoving = false;   // 動作完了にする
    }
    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        Invoke("MovingCheck", 4f);  //動作チェック
        if (!isRised)
        {   //上昇済みでない場合
            isRised = true;       // 上昇済みとする
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //上昇する
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //ワイヤー上昇する
            }
            //移動SEを再生する
            movementSE.Play();
        }
        else
        {   //上昇済みの場合
            isRised = false;    // 下降済みとする
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //下降する
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //ワイヤーも下降する
            }
            //移動SEを再生する
            movementSE.Play();
        }
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
                isRised = false;    // 下降済みとする
                if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
                {
                    tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //下降する
                    if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //ワイヤーも下降する
                }
                //移動SEを再生する
                movementSE.Play();
            }
        }
        else
        {
            Invoke("MovingCheck", 4f);  //動作チェック
            if (!isRised)
            {   //上昇済みでない場合
                isRised = true;       // 上昇済みとする
                if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
                {
                    tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //上昇する
                    if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //ワイヤー上昇する
                }
                //移動SEを再生する
                movementSE.Play();
            }
        }
    }
}