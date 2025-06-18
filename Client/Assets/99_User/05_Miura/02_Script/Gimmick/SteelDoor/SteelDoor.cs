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
    /// <summary>
    /// ドアを開ける関数
    /// </summary>
    public void Open()
    {
        //2秒かけて上に移動する
        this.transform.DOMoveY(5f, 1f); 
    }

    /// <summary>
    /// ドアを閉じる関数
    /// </summary>
    public void Close()
    {
        //強制的に処理を停止する
        this.transform.DOKill();
    }
}
