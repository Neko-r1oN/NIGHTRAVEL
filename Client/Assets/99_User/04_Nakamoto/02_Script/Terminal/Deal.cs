//--------------------------------------------------------------
// 取引ターミナル [ Deal.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Deal : TerminalBase
{
    //--------------------------------
    // フィールド

    /// <summary>
    /// ダメージ割合
    /// </summary>
    private const float DEAL_RATE = 0.3f;

    //--------------------------------
    // メソッド

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Deal;
    }

    /// <summary>
    /// 起動リクエスト
    /// </summary>
    public override async void BootRequest()
    {
        int damage = (int)((float)CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().MaxHP * DEAL_RATE);
        if (CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp - damage <= 0) return;
        DealDamage(damage);

        base.BootRequest();
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする
        usingText.text = "IN USE";

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID-1].State = EnumManager.TERMINAL_STATE.Success;

        // レリック生成リクエスト
        SuccessRequest();

        // ターミナル非表示
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

    /// <summary>
    /// 取引ダメージ
    /// </summary>
    private void DealDamage(int damage)
    {
        // 最大体力割合のダメージを受ける
        CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().ApplyDamage(damage);
        UIManager.Instance.PopDamageUI(damage, CharacterManager.Instance.PlayerObjSelf.transform.position, true);
    }
}
