using UnityEngine;

public class EnemyAnimationEventController : MonoBehaviour
{
    [SerializeField] 
    EnemyBase m_EnemyBase;
    
    /// <summary>
    /// 攻撃のイベント通知
    /// </summary>
    public void OnAttackEvent()
    {
        m_EnemyBase.OnAttackAnimEvent();
    }
}
