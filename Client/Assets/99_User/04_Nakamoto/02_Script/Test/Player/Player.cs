//--------------------------------------------------------------
// プレイヤー抽象親クラス [ Player.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

abstract public class Player : MonoBehaviour
{
    // プレイヤーに共通する関数をここに記載する

    /// <summary>
    /// 
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="position"></param>
    abstract public void ApplyDamage(float damage, Vector3 position);
}
