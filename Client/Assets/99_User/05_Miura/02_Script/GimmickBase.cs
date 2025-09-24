//======================
// ギミック全体の親クラス
// Auther:y-miura
// Date:2025/07/01
//======================

using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    [SerializeField]
    bool triggerOnce;   // 起動できるのが一度きりかどうか

    [SerializeField]
    bool requiresReactivation;  // マスタクライアントに切り替わったときに、再起動が必要かどうか

    // 識別用ID
    string uniqueId;
    public string UniqueId {  get { return uniqueId; } set { uniqueId = value; } }

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

    /// <summary>
    /// [リアルタイム同期用] ギミック更新
    /// </summary>
    public virtual void UpdateGimmick(GimmickData gimmickData)
    {
        transform.DOMove(gimmickData.Position, CharacterManager.Instance.UpdateSec).SetEase(Ease.Linear);
    }
}
