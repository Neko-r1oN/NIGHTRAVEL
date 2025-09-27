//===================
// エレベータースクリプト
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
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

    [SerializeField] AudioClip moveSE;
    [SerializeField] AudioClip arrivedSE;

    AudioSource audioSource;

    // エレベーターの目標地点
    Vector2 riseEndPos;
    Vector2 descentEndPos;

    // ワイヤーの目標地点
    Vector2 riseEndWirePos;
    Vector2 descentEndWirePos;

    private void Start()
    {
        // 下降済みの状態から始まるのを想定した目標地点を設定
        riseEndPos = transform.position + Vector3.up * risePow;
        descentEndPos = transform.position;
        riseEndWirePos = wire.transform.position + Vector3.up * risePow;
        descentEndWirePos = wire.transform.position;

        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (isBroken)
        {
            tweener.Kill();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // エレベーターに乗っているキャラクターを子オブジェクトに設定
        var obj = collision.transform.gameObject;
        if (obj.tag == "Player" && obj == CharacterManager.Instance.PlayerObjSelf
            || collision.transform.tag == "Enemy")
        {
            collision.transform.SetParent(transform);
        }

        if (isBroken == true || isMoving == true || isPowerd == false) return;  // 電源offまたはエレベーター動作中の場合処理しない
        // プレイヤーがエレベーター内に入った場合
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isMoving = true;    // 動作中にする
            Invoke("InvokeTurnOnPowerRequest", 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // エレベーターに乗っていたキャラクターを子オブジェクトから外す
        if (collision.transform.tag == "Player" || collision.transform.tag == "Enemy")
        {
            var obj = collision.transform.gameObject;
            if (obj.tag == "Player" && obj != CharacterManager.Instance.PlayerObjSelf) return;
            if (collision.transform.parent == transform) collision.transform.parent = null;
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
            //arrivalSE.Play();
            audioSource.PlayOneShot(arrivedSE);
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
    /// 上昇する
    /// </summary>
    void MoveUp()
    {
        if (isRised || (Vector2)transform.position == riseEndPos) return;
        Invoke("MovingCheck", moveSpeed);  //動作チェック
        isRised = true;       // 上昇済みとする
        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            tweener = this.transform.DOMove(riseEndPos, moveSpeed);
            if (wire) wire.transform.DOMove(riseEndWirePos, moveSpeed);
        }
        //移動SEを再生する
        movementSE.Play();
    }

    /// <summary>
    /// 下降する
    /// </summary>
    void MoveDown()
    {
        if (!isRised || (Vector2)transform.position == descentEndPos) return;
        Invoke("MovingCheck", moveSpeed);  //動作チェック
        isRised = false;    // 下降済みとする
        isMoving = true;    // 動作中にする

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            tweener = this.transform.DOMove(descentEndPos, moveSpeed);
            if (wire) wire.transform.DOMove(descentEndWirePos, moveSpeed);
        }
        //移動SEを再生する
        movementSE.Play();
    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        if (!isRised) MoveUp();
        else MoveDown();
    }

    /// <summary>
    /// 昇降ボタン処理
    /// </summary>
    public void MoveButton(bool type)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // 電源offまたはエレベーター動作中の場合処理しない

        if (type == false) MoveDown();
        else MoveUp();
    }

    /// <summary>
    /// マスタ切り替え時の再起動処理
    /// </summary>
    public override void Reactivate()
    {
        CancelInvoke("MovingCheck");

        if (isRised)
        {
            transform.position = riseEndPos;
            if (wire) wire.transform.position = riseEndWirePos;
        }
        else
        {
            transform.position = descentEndPos;
            if (wire) wire.transform.position = descentEndWirePos;
        }
        isMoving = false;
    }
}