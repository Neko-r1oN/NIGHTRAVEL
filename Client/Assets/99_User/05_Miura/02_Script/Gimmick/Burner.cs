//===================
// �o�[�i�[�Ɋւ��鏈��
// Aouther:y-miura
// Date:2025/07/07
//===================

using System.Collections;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    [SerializeField] GameObject flame;
    bool isFlame;

    private void Start()
    {
        //invokerepeting��Ignition���Ă�
        //3�b�Ԋu�œ_������������肷��
        InvokeRepeating("Ignition", 0.1f, 3);
    }

    private void Update()
    {

    }

    /// <summary>
    /// �G�ꂽ�I�u�W�F�N�g�ɉ�����ʂ�t�^���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = GetComponent<PlayerBase>();

            Debug.Log("�v���C���[�ɉ����Ԃ�t�^");
        }

        if (collision.CompareTag("Enemy"))
        {
            enemy = GetComponent<EnemyBase>();

            Debug.Log("�G�ɉ����Ԃ�t�^");
        }
    }

    /// <summary>
    /// ����_������������肷�鏈��
    /// </summary>
    private void Ignition()
    {
        if(isFlame==true)
        {//isFlame��true��������
            //flame���A�N�e�B�u��Ԃɂ���
            flame.SetActive(false);
            isFlame = false;
        }
        else if(isFlame==false)
        {//isFlame��false��������
            //flame���A�N�e�B�u��Ԃɂ���
            flame.SetActive(true); 
            isFlame = true;
        }
    }

    public override void TurnOnPower(int triggerID)
    {

    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }

}
