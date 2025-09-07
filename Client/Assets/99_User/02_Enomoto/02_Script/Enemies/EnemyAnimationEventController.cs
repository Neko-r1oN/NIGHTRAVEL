using UnityEngine;

public class EnemyAnimationEventController : MonoBehaviour
{
    [SerializeField] 
    EnemyBase m_EnemyBase;

    #region �U���p�^�[���P

    /// <summary>
    /// �U���̃C�x���g
    /// </summary>
    public void OnAttackEvent()
    {
        m_EnemyBase.OnAttackAnimEvent();
    }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g
    /// </summary>
    public void OnEndAttackEvent()
    {
        m_EnemyBase.OnEndAttackAnimEvent();
    }

    #endregion

    #region �U���p�^�[���Q

    /// <summary>
    /// �U���̃C�x���g
    /// </summary>
    public void OnAttack2Event()
    {
        m_EnemyBase.OnAttackAnim2Event();
    }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g
    /// </summary>
    public void OnEndAttack2Event()
    {
        m_EnemyBase.OnEndAttackAnim2Event();
    }

    #endregion

    /// <summary>
    /// �ړ�����A�j���C�g�C�x���g
    /// </summary>
    public void OnMoveAnimEvent()
    {
        m_EnemyBase.OnMoveAnimEvent();
    }

    /// <summary>
    /// �ړ��I���A�j���[�V�����C�x���g�ʒm
    /// </summary>
    public void OnEndMoveAnimationEvent()
    {
        m_EnemyBase.OnEndMoveAnimEvent();
    }

    /// <summary>
    /// �X�|�[���A�j���̃C�x���g
    /// </summary>
    public void OnSpawnEvent()
    {
        m_EnemyBase.OnSpawnAnimEvent();
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ��̃C�x���g
    /// </summary>
    public void OnEndSpawnEvent()
    {
        m_EnemyBase.OnEndSpawnAnimEvent();
    }
}
