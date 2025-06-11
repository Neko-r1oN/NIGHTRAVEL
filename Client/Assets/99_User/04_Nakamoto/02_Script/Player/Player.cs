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
    /// ��_������(�m�b�N�o�b�N�L)
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <param name="position">�U�������I�u�W�F�̈ʒu</param>
    abstract public void ApplyDamage(int damage, Vector3 position);

    /// <summary>
    /// ��_������(�m�b�N�o�b�N��)
    /// </summary>
    /// <param name="dealer"></param>
    /// <param name="damage"></param>
    abstract public void DealDamage(GameObject dealer, int damage);

    /// <summary>
    /// �o���l�l������
    /// </summary>
    /// <param name="exp">�l���o���l</param>
    abstract public void GetExp(int exp);

    /// <summary>
    /// �X�e�[�^�X�ϓ�����
    /// </summary>
    /// <param name="statusID">����������X�e�[�^�XID</param>
    /// <param name="value">�����l</param>
    abstract public void ChangeStatus(int statusID, int value);
}
