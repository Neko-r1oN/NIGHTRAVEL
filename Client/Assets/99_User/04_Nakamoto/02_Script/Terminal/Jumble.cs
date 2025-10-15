//--------------------------------------------------------------
// レリックジャンブルターミナル [ Jumble.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Jumble : TerminalBase
{
    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Jumble;
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする
        usingText.text = "IN USE";

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID - 1].State = EnumManager.TERMINAL_STATE.Success;

        // 成功処理
        if(RoomModel.Instance.IsMaster) SuccessRequest();

        // ターミナル非表示
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }
}
