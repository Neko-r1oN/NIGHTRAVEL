//===================
// �o�[�i�[�Ɋւ��鏈��
// Aouther:y-miura
// Date:2025/07/07
//===================
using UnityEngine;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    StatusEffectController statusEffectController;
    [SerializeField] GameObject flame;
    bool isFlame;

    //�I�u�W�F�N�g�̋N����Ԃ�ID
    //triggerID���N����Ԃ�ID�ɂȂ�
    public enum Power_ID
    {
        ON = 0,
        OFF
    };

    private void Start()
    {
        //3�b�Ԋu�œ_�΂Ə��΂��J��Ԃ�
        InvokeRepeating("Ignition", 0.1f, 3);
    }

    private void Update()
    {

    }

    /// <summary>
    /// �G�ꂽ�v���C���[/�G�ɉ�����ʂ�t�^���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�G�ꂽ�I�u�W�F�N�g�̃^�O���uPlayer�v��������
            player = GetComponent<PlayerBase>();
            statusEffectController = collision.gameObject.GetComponent<StatusEffectController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController��GetComponent����

            statusEffectController.ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Burn); //�v���C���[�ɉ����Ԃ�t�^
            Debug.Log("�v���C���[�ɉ����Ԃ�t�^");
        }

        if (collision.CompareTag("Enemy"))
        {//�G�ꂽ�I�u�W�F�N�g�̃^�O���uEnemy�v��������
            enemy = GetComponent<EnemyBase>();
            statusEffectController = collision.gameObject.GetComponent<StatusEffectController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController���擾����

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
