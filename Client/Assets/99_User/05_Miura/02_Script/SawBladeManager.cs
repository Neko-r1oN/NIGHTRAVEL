using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SawBladeManager : GimmickBase
{
    Vector2 pos;

    [SerializeField] GameObject sparkObj;
    [SerializeField] SawBlade sawBlade;

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
        //    //Sequenceのインスタンスを作成
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
        sparkObj.SetActive(true);
        MoveBlade();
        sawBlade.StateRotet();
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
