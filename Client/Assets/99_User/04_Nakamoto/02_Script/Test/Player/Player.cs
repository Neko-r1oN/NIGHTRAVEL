//--------------------------------------------------------------
// �v���C���[���ېe�N���X [ Player.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

abstract public class Player : MonoBehaviour
{
    // �v���C���[�ɋ��ʂ���֐��������ɋL�ڂ���

    /// <summary>
    /// 
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <param name="position"></param>
    abstract public void ApplyDamage(float damage, Vector3 position);
}
