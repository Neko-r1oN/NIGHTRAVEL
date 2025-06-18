//=========================================
//鋼鉄製自動ドアの開閉を判定するセンサーのスクリプト
//Aouther:y-miura
//Date:2025/06/18
//=========================================
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    PlayerBase playerBase;
    SteelDoor steelDoor;

    /// <summary>
    /// センサー範囲内に入ったらドアを開ける処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//プレイヤーが鋼鉄製自動ドアのセンサー範囲に入ったら
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            //playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoorクラスのOpen関数を呼び出す
            steelDoor.Open();
        }
    }

    /// <summary>
    /// センサー範囲から離れたらドアを閉じる処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//プレイヤーが鋼鉄製自動ドアのセンサー範囲から離れたら
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            //playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoorクラスのClose関数を呼び出す
            steelDoor.Close();
        }
    }
}
