//========================
//����I�u�W�F�N�g�̐e�N���X
//Author:y-Miura
//date:2025/03/17
//========================

using UnityEngine;

abstract public class ObjectBase : MonoBehaviour
{
    /// <summary>
    /// ����I�u�W�F�N�g�̃_���[�W�֐�
    /// </summary>
    abstract public void ApplyDamage();

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    abstract public void FadeFragment(Transform fragment);
}
