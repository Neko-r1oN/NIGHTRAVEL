//--------------------------------------------------------------
// エリート出現ターミナル [ Elite.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Elite : TerminalBase
{
    //--------------------------------
    // フィールド

    [SerializeField] private Transform minSpawnPos;
    [SerializeField] private Transform maxSpawnPos;

    //--------------------------------
    // メソッド

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Elite;
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Active;
        else
        {   // オフライン状態
            InvokeRepeating("CountDown", 1, 1);
            SpawnManager.Instance.TerminalGenerateEnemy(SPAWN_ENEMY_NUM, terminalID,
                new Vector2(minSpawnPos.position.x, minSpawnPos.position.y), new Vector2(maxSpawnPos.position.x, maxSpawnPos.position.y), true);
            return;
        }

        // マスターの場合はCountDownを開始。その他は初期カウントを設定
        if (RoomModel.Instance.IsMaster)
        {
            InvokeRepeating("CountDown", 1, 1);
            SpawnManager.Instance.TerminalGenerateEnemy(SPAWN_ENEMY_NUM, terminalID,
                new Vector2(minSpawnPos.position.x, minSpawnPos.position.y), new Vector2(maxSpawnPos.position.x, maxSpawnPos.position.y), true);
        }
        else
            timerText.text = currentTime.ToString();
    }

    /// <summary>
    /// 失敗処理
    /// </summary>
    public override void FailureTerminal()
    {
        base.FailureTerminal();

        // 自身の端末から生成された敵の削除
        CharacterManager.Instance.DeleteTerminalEnemy(terminalID);
    }
}
