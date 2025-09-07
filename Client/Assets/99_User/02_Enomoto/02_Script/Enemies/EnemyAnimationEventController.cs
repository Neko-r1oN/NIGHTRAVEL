using UnityEngine;

public class EnemyAnimationEventController : MonoBehaviour
{
    [SerializeField] 
    EnemyBase m_EnemyBase;

    #region 攻撃パターン１

    /// <summary>
    /// 攻撃のイベント
    /// </summary>
    public void OnAttackEvent()
    {
        m_EnemyBase.OnAttackAnimEvent();
    }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント
    /// </summary>
    public void OnEndAttackEvent()
    {
        m_EnemyBase.OnEndAttackAnimEvent();
    }

    #endregion

    #region 攻撃パターン２

    /// <summary>
    /// 攻撃のイベント
    /// </summary>
    public void OnAttack2Event()
    {
        m_EnemyBase.OnAttackAnim2Event();
    }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント
    /// </summary>
    public void OnEndAttack2Event()
    {
        m_EnemyBase.OnEndAttackAnim2Event();
    }

    #endregion

    /// <summary>
    /// 移動するアニメイトイベント
    /// </summary>
    public void OnMoveAnimEvent()
    {
        m_EnemyBase.OnMoveAnimEvent();
    }

    /// <summary>
    /// 移動終了アニメーションイベント通知
    /// </summary>
    public void OnEndMoveAnimationEvent()
    {
        m_EnemyBase.OnEndMoveAnimEvent();
    }

    /// <summary>
    /// スポーンアニメのイベント
    /// </summary>
    public void OnSpawnEvent()
    {
        m_EnemyBase.OnSpawnAnimEvent();
    }

    /// <summary>
    /// スポーンアニメーションが終了したときのイベント
    /// </summary>
    public void OnEndSpawnEvent()
    {
        m_EnemyBase.OnEndSpawnAnimEvent();
    }
}
