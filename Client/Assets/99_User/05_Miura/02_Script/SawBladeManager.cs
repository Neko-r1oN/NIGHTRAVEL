using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class SawBladeManager : GimmickBase
{
    Vector2 pos;

    [SerializeField] GameObject sparkObj;
    [SerializeField] SawBlade sawBlade;
    [SerializeField] AudioSource sawBladeSE;

    // 加力値
    public float addPower;

    // 移動速度
    public float moveSpeed;

    // 電源判定
    public bool isPowerd;

    void Start()
    {
        isPowerd = true;

        // このゲームオブジェクトのポジションを取得
        pos = this.transform.position;

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) TurnOnPower();
    }

    /// <summary>
    /// 丸のこ移動関数
    /// </summary>
    private void MoveBlade()
    {
        //Sequenceのインスタンスを作成
        var sequence = DOTween.Sequence();

        transform.DOMoveX((pos.x - addPower), 1)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnStepComplete(() =>
                    {
                        // ループ1往復ごとに呼ばれる
                        Vector3 scale = transform.localScale;
                        scale.x *= -1;
                        transform.localScale = scale;
                    });
    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        sawBladeSE.Play();

        //火花を散らす
        sparkObj.SetActive(true);

        //丸のこを移動させる
        MoveBlade();

        //丸のこを回転させる
        sawBlade.StateRotet(); //SawBladeクラスのStateRotet関数を呼び出す
    }

    /// <summary>
    /// ギミック再起動処理
    /// </summary>
    public override void Reactivate()
    {
        transform.position = pos;
        TurnOnPower();
    }
}
