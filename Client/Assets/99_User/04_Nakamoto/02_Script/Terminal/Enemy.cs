//--------------------------------------------------------------
// エネミー出現ターミナル [ EnemyTerminal.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Enemy : TerminalBase
{
    //--------------------------------
    // フィールド



    //--------------------------------
    // メソッド

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Enemy;
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        base.BootTerminal();

        //++ 起動リクエストをサーバーに送信
    }
}
