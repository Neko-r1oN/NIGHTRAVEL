//--------------------------------------------------------------
// プレイヤー抽象親クラス [ Player.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

abstract public class Player : MonoBehaviour
{
    // プレイヤーに共通する関数をここに記載する

    /// <summary>
    /// 攻撃処理
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// 被ダメ処理
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="position">攻撃したオブジェの位置</param>
    abstract public void ApplyDamage(int damage, Vector3 position);
}
