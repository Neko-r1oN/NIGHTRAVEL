using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using System;
using Unity.VisualScripting;
using UnityEngine;

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

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) Invoke("Request", 0.5f);
    }

    public void Request()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
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

        if(addPower<=0)
        {
            sparkObj.SetActive(false);
        }
    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        //火花を散らす
        sparkObj.SetActive(true);

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) MoveBlade();

        //丸のこを回転させる
        sawBlade.StateRotet(); //SawBladeクラスのStateRotet関数を呼び出す

        if(addPower==0)
        {//addpowerが0か0以下だったら

            //火花を非表示にする
            sparkObj.SetActive(false);

            //丸のこを移動させる
            MoveBlade();
        }
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
