//--------------------------------------------------------------
// 取引ターミナル [ Deal.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Deal : TerminalBase
{
    //--------------------------------
    // フィールド

    /// <summary>
    /// ダメージ割合
    /// </summary>
    private const float DEAL_RATE = 0.5f;

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
        base.BootRequest();

        DealDamage();
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする
        TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Success;

        // レリック生成リクエスト
        GiveRewardRequest();
    }

    /// <summary>
    /// 取引ダメージ
    /// </summary>
    private void DealDamage()
    {
        // 最大体力割合のダメージを受ける
        int damage = (int)(CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().MaxHP * DEAL_RATE);
        damage = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp - damage <= 0 ? 1 : damage;

        CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp -= damage;
        UIManager.Instance.PopDamageUI(damage, CharacterManager.Instance.PlayerObjSelf.transform.position, true);
    }
}
