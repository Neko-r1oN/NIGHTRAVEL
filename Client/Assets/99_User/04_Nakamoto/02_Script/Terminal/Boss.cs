//--------------------------------------------------------------
// ボス出現ターミナル [ Jumble.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Boss : TerminalBase
{
    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Boss;
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Success;

        // マスタクライアント以外は処理をしない
        if(!RoomModel.Instance)
        {
            SpawnManager.Instance.SpawnBoss();
        }
        else if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        // ボス生成処理
        SpawnManager.Instance.SpawnBoss();
    }
}
