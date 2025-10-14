//--------------------------------------------------------------
// �v���C���[�G�t�F�N�g [ PlayerEffect.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    //------------------------------
    // �t�B�[���h

    /// <summary>
    /// �_�b�V���G�t�F�N�g
    /// </summary>
    [SerializeField] private ParticleSystem dashEffect = null;

    /// <summary>
    /// �r�[���G�t�F�N�g
    /// </summary>
    [SerializeField] private GameObject beamEffect = null;

    /// <summary>
    /// �A�j���[�^�[
    /// </summary>
    [SerializeField]�@private Animator animator;

    private const int ANIMATION_ID_DASH = 2;

    //------------------------------
    // ���\�b�h

    /// <summary>
    /// �X�V����
    /// </summary>
    void Update()
    {
        // �����Ă��鎞�ɓy�����N����
        if (animator.GetInteger("animation_id") == ANIMATION_ID_DASH)
        {
            if (!dashEffect.isPlaying) dashEffect.Play();
        }
        else
        {
            if (dashEffect.isPlaying) dashEffect.Stop();
        }
    }

    /// <summary>
    /// �r�[���G�t�F�N�g�\���ؑ�
    /// </summary>
    /// <param name="isActive"></param>
    public void BeamEffectActive(bool isActive)
    {
        if (beamEffect != null) beamEffect.SetActive(isActive);
    }
}
