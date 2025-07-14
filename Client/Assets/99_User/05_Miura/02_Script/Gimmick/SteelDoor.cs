//=================
//�|�S�������h�A�̃X�N���v�g
//Aouther:y-miura
//Date:20025/06/23
//=================

using DG.Tweening;
using System.Configuration;
using UnityEngine;

public class SteelDoor : GimmickBase
{
    [SerializeField] GameObject doorObj;
    Vector2 initPos = Vector2.zero;//�����ʒu
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //�����ʒu��ݒ�
        initPos = transform.position;
    }

    /// <summary>
    /// �|�S�������h�A���J������
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerStay2D(Collider2D collision)
    {//�Z���T�[�����ɂ��̂��G�ꂽ��
        if (collision.CompareTag("Player"))
        {//�uPlayer�v�^�O���t�������̂��G�ꂽ��
            //�h�A���J��
            doorObj.transform.DOMoveY(this.transform.position.y+5, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�uPlayer�v�^�O���t�������̂����ꂽ��
         //�h�A�����
            doorObj.transform.DOMoveY(initPos.y, 0.5f);
        }
    }

    public override void TurnOnPower(int triggerID)
    {
        isPowerd = true;
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }

}