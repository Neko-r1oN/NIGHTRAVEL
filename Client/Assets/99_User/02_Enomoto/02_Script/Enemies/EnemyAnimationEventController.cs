using UnityEngine;

public class EnemyAnimationEventController : MonoBehaviour
{
    [SerializeField] 
    EnemyBase m_EnemyBase;
    
    /// <summary>
    /// 攻撃のイベント
    /// </summary>
    public void OnAttackEvent()
    {
        m_EnemyBase.OnAttackAnimEvent();
    }

    /// <summary>
    /// スポーンアニメのイベント
    /// </summary>
    public void OnSpawnEvent()
    {
        m_EnemyBase.OnEndSpawnAnim();
    }
}
