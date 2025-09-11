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

    #region �U���p�^�[���R

    /// <summary>
    /// �U���̃C�x���g
    /// </summary>
    public void OnAttack3Event()
    {
        m_EnemyBase.OnAttackAnim3Event();
    }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g
    /// </summary>
    public void OnEndAttack3Event()
    {
        m_EnemyBase.OnEndAttackAnim3Event();
    }

    #endregion

    #region �U���p�^�[���S

    /// <summary>
    /// �U���̃C�x���g
    /// </summary>
    public void OnAttack4Event()
    {
        m_EnemyBase.OnAttackAnim4Event();
    }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g
    /// </summary>
    public void OnEndAttack4Event()
    {
        m_EnemyBase.OnEndAttackAnim4Event();
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
