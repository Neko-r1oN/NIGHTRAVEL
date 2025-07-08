using UnityEngine;

public class EnemyAnimationEventController : MonoBehaviour
{
    [SerializeField] 
    EnemyBase m_EnemyBase;
    
    /// <summary>
    /// �U���̃C�x���g
    /// </summary>
    public void OnAttackEvent()
    {
        m_EnemyBase.OnAttackAnimEvent();
    }

    /// <summary>
    /// �X�|�[���A�j���̃C�x���g
    /// </summary>
    public void OnSpawnEvent()
    {
        m_EnemyBase.OnEndSpawnAnim();
    }
}
