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
    [SerializeField] GameObject doorObj;
    Vector2 initPos = Vector2.zero; //�h�A�̏����ʒu

    private void Start()
    {
        //�h�A�̏����ʒu����
        initPos = transform.position;
    }

    /// <summary>
    /// �Z���T�[�͈͓��ɓ�������h�A���J���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            doorObj.transform.DOMoveY(5f, 0.5f);
        }
    }

    /// <summary>
    /// �Z���T�[�͈͓����痣�ꂽ��h�A����鏈��
    /// </summary>
    /// <param name="collision">�G��Ă����I�u�W�F�N�g</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            doorObj.transform.DOMoveY(initPos.y, 0.5f);
        }
    }
}
