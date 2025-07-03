//===================
// 感圧版スクリプト
// Author:Nishiura
// Date:2025/07/01
//===================
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    // 押下判定変数
    private bool isPushed = false;

    // 関連付けられたギミックリスト
    [SerializeField] List<GameObject> linkedGimmick = new List<GameObject>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーが触れた場合かつ押された状態でない場合
        if (collision.transform.tag == "Player" && isPushed != true)
        {
            isPushed = true;
            PowerOn();
            Debug.Log("Plate Activated");
        }
    }

    /// <summary>
    /// 電源オン処理
    /// </summary>
    private void PowerOn()
    {
        foreach (GameObject gimmick in linkedGimmick)
        {
            switch (gimmick.name)
            {
                case "SteelDoor_Set":
                    gimmick.GetComponent<SteelDoor>().TurnOnPower();
                    break;

                case "Belt Conveyor":

                    break;

                case "Fan":

                    break;

                case "PressMachine":
                    gimmick.GetComponent<PressMachine>().TurnOnPower();
                    break;

                case "SawBlade":
                    gimmick.GetComponent<SawBlade>().TurnOnPower();
                    break;

                default:
                    break;
            }
        }
    }
}
