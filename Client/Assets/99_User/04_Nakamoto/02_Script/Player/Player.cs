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
    /// 被ダメ処理(ノックバック有)
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="position">攻撃したオブジェの位置</param>
    abstract public void ApplyDamage(int damage, Vector3 position);

    /// <summary>
    /// 被ダメ処理(ノックバック無)
    /// </summary>
    /// <param name="dealer"></param>
    /// <param name="damage"></param>
    abstract public void DealDamage(GameObject dealer, int damage);

    /// <summary>
    /// 経験値獲得処理
    /// </summary>
    /// <param name="exp">獲得経験値</param>
    abstract public void GetExp(int exp);

    /// <summary>
    /// ステータス変動処理
    /// </summary>
    /// <param name="statusID">増減させるステータスID</param>
    /// <param name="value">増減値</param>
    abstract public void ChangeStatus(int statusID, int value);
}
