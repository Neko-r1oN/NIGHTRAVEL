//======================
//�|�S�������h�A�̃X�N���v�g
//Aouther:y-miura
//Date:2025/06/18
//======================

using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SteelDoor : MonoBehaviour
{
    /// <summary>
    /// �h�A���J����֐�
    /// </summary>
    public void Open()
    {
        //2�b�����ď�Ɉړ�����
        this.transform.DOMoveY(5f, 1f); 
    }

    /// <summary>
    /// �h�A�����֐�
    /// </summary>
    public void Close()
    {
        //�����I�ɏ������~����
        this.transform.DOKill();
    }
}
