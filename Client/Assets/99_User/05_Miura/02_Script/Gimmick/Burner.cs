//===================
// �o�[�i�[�Ɋւ��鏈��
// Aouther:y-miura
// Date:2025/07/07
//===================
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    DebuffController statusEffectController;
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
        TuenOnPowerByMaster();
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
            statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController��GetComponent����

            statusEffectController.ApplyStatusEffect(DEBUFF_TYPE.Burn); //�v���C���[�ɉ����Ԃ�t�^
            Debug.Log("�v���C���[�ɉ����Ԃ�t�^");
        }

        if (collision.CompareTag("Enemy"))
        {//�G�ꂽ�I�u�W�F�N�g�̃^�O���uEnemy�v��������
            enemy = GetComponent<EnemyBase>();
            statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController���擾����

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
            //NavMeshObstacle�R���|�[�l���g���A�N�e�B�u��Ԃɂ���
            GetComponent<NavMeshObstacle>().enabled = false;

            //flame���A�N�e�B�u��Ԃɂ���
            flame.SetActive(false);
            isFlame = false;
        }
        else if(isFlame==false)
        {//isFlame��false��������
         //NavMeshObstacle�R���|�[�l���g���A�N�e�B�u��Ԃɂ���
            GetComponent<NavMeshObstacle>().enabled = true;

            //flame���A�N�e�B�u��Ԃɂ���
            flame.SetActive(true); 
            isFlame = true;
        }
    }

    public override void TurnOnPower(int triggerID)
    {
        //3�b�Ԋu�œ_�΂Ə��΂��J��Ԃ�
        InvokeRepeating("Ignition", 0.1f, 3);
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }

}
