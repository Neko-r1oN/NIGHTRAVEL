//========================
//����I�u�W�F�N�g�̐e�N���X
//Author:y-Miura
//date:2025/03/17
//========================

using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.UIElements;

abstract public class ObjectBase : GimmickBase
{
    /// <summary>
    /// ����I�u�W�F�N�g�̃_���[�W�֐�
    /// </summary>
    abstract protected void ApplyDamage ();

    abstract public void DestroyFragment(Transform obj);

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    abstract public void FadeFragment(Transform fragment);
}
