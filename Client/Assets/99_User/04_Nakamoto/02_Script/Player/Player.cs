//--------------------------------------------------------------
// �v���C���[���ېe�N���X [ Player.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

abstract public class Player : MonoBehaviour
{
    // �v���C���[�ɋ��ʂ���֐��������ɋL�ڂ���

    /// <summary>
    /// �U������
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// ��_������
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <param name="position">�U�������I�u�W�F�̈ʒu</param>
    abstract public void ApplyDamage(int damage, Vector3 position);
}
