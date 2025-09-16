//======================
// ギミック全体の親クラス
// Auther:y-miura
// Date:2025/07/01
//======================

using System;
using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    [SerializeField]
    bool triggerOnce;

    // 識別用ID
    int uniqueId;
    public int UniqueId {  get { return uniqueId; } set { uniqueId = value; } }

    // ギミックの状態 (true：ON, false：OFF)
    protected bool isBoot = false;
    public bool IsBoot { get { return isBoot; } set { isBoot = value; } }

    /// <summary>
    /// ギミックの起動
    /// </summary>
    /// <param name="triggerID"></param>

    public virtual void TurnOnPower()
    {
        isBoot = true;
    }

    /// <summary>
    /// ギミック起動リクエスト
    /// </summary>
    /// <param name="player">起動したプレイヤー</param>
    public async void TurnOnPowerRequest(GameObject player)
    {
        // オフライン用
        if (!RoomModel.Instance)
        { 
            TurnOnPower();
        }
        // マルチプレイ中 && 起動した人が自分自身の場合
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // サーバーに対してリクエスト処理
            await RoomModel.Instance.BootGimmickAsync(uniqueId, triggerOnce);
        }
    }

    /// <summary>
    /// [マスタクライアントが自身に切り替わったときに呼ばれる]
    /// ギミック再起動処理
    /// </summary>
    public virtual void Reactivate()
    {
        Debug.Log($"{gameObject.name}が再起動した。");
    }
}
