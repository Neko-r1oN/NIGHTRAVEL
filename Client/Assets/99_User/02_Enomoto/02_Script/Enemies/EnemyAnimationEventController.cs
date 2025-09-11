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

    #region 攻撃パターン３

    /// <summary>
    /// 攻撃のイベント
    /// </summary>
    public void OnAttack3Event()
    {
        m_EnemyBase.OnAttackAnim3Event();
    }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント
    /// </summary>
    public void OnEndAttack3Event()
    {
        m_EnemyBase.OnEndAttackAnim3Event();
    }

    #endregion

    #region 攻撃パターン４

    /// <summary>
    /// 攻撃のイベント
    /// </summary>
    public void OnAttack4Event()
    {
        m_EnemyBase.OnAttackAnim4Event();
    }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント
    /// </summary>
    public void OnEndAttack4Event()
    {
        m_EnemyBase.OnEndAttackAnim4Event();
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
