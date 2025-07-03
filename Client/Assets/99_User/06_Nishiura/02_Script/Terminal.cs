//===================
// 端末スクリプト
// Author:Nishiura
// Date:2025/07/01
//===================
using Unity.VisualScripting;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    // プレイヤーが端末に触れているかの判定変数
    private bool isPlayerIn = false;
    // 使用判定
    private bool isUsed = false;
    // 端末の種別
    public int terminalType;

    // 端末タイプ列挙型
    public enum TerminalCode 
    {
        None = 0,
        Type_Enemy,
        Type_Speed,
        Type_Deal,
        Type_Recycle,
        Type_Jumble,
        Type_Return,
        Type_Elite
    }

    private void Start()
    {

    }

    private void Update()
    {
        // Eキー入力かつプレイヤーが端末に触れている場合かつその端末が未使用である場合、端末を起動
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // 端末を起動
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーが端末付近に接近した場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = true;  // 触れたこととする
            Debug.Log("You Touched Terminal");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーが端末から離れた場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = false; // 触れていないこととする
            Debug.Log("No Terminal");
        }
    }

    /// <summary>
    /// 端末起動処理
    /// </summary>
    private void BootTerminal()
    {
        // 端末タイプで処理を分ける
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // 敵生成の場合
                isUsed = true;  // 使用済みにする
                SpawnManager.Instance.GenerateEnemy(1);   // 敵生成
                break;
            case (int)TerminalCode.Type_Speed:
                // スピードの場合

                break;
            case (int)TerminalCode.Type_Deal:
                // 取引の場合

                break;
            case (int)TerminalCode.Type_Jumble:
                // ごちゃまぜの場合

                break;
            case (int)TerminalCode.Type_Elite:
                // エリート敵生成の場合

                break;
            case (int)TerminalCode.Type_Recycle:
                // リサイクルの場合

                break;
            case (int)TerminalCode.Type_Return:
                // 再帰の場合

                break;
        }
    }
}
