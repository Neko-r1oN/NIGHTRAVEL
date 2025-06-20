//======================
//鋼鉄製自動ドアのスクリプト
//Aouther:y-miura
//Date:2025/06/18
//======================

using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SteelDoor : MonoBehaviour
{
    [SerializeField] GameObject doorObj;
    Vector2 initPos = Vector2.zero; //ドアの初期位置

    private void Start()
    {
        //ドアの初期位置を代入
        initPos = transform.position;
    }

    /// <summary>
    /// センサー範囲内に入ったらドアを開ける処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            doorObj.transform.DOMoveY(5f, 0.5f);
        }
    }

    /// <summary>
    /// センサー範囲内から離れたらドアを閉じる処理
    /// </summary>
    /// <param name="collision">触れていたオブジェクト</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            doorObj.transform.DOMoveY(initPos.y, 0.5f);
        }
    }
}
