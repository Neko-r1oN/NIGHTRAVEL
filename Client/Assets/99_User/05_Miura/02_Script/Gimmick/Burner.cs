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
    [SerializeField] AudioSource flameSE;

    NavMeshObstacle navMeshObstacle;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;

    bool isFlame;
    const float repeatRate = 3f;
    float timer = 0;        // �}�X�^�N���C�A���g�����g�ɐ؂�ւ�����Ƃ��p

    private void Start()
    {
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        // �I�t���C�� or �}���`�v���C���Ƀ}�X�^�N���C�A���g�̏ꍇ
        if(!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            //3�b�Ԋu�œ_�΂Ə��΂��J��Ԃ�
            InvokeRepeating("RequestActivateGimmick", 0.1f, repeatRate);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    /// <summary>
    /// �G�ꂽ�v���C���[/�G�ɉ�����ʂ�t�^���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�G�ꂽ�I�u�W�F�N�g�̃^�O���uPlayer�v��������

            // �_���[�W��K�p������Ώۂ������̑���L�����̏ꍇ
            if(collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
            {
                player = GetComponent<PlayerBase>();
                statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController��GetComponent����

                statusEffectController.ApplyStatusEffect(DEBUFF_TYPE.Burn); //�v���C���[�ɉ����Ԃ�t�^
                Debug.Log("�v���C���[�ɉ����Ԃ�t�^");
            }
        }

        if (collision.CompareTag("Enemy"))
        {//�G�ꂽ�I�u�W�F�N�g�̃^�O���uEnemy�v��������

            // �I�t���C���� or �}���`�v���C���Ń}�X�^�N���C�A���g�̏ꍇ
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                enemy = GetComponent<EnemyBase>();
                statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //�G�ꂽ�I�u�W�F�N�g��StatusEffectController���擾����

                Debug.Log("�G�ɉ����Ԃ�t�^");
            }
        }
    }

    /// <summary>
    /// ����_������������肷�鏈��
    /// </summary>
    private void Ignition()
    {
        timer = 0;
        if (isFlame==true)
        {
            flameSE.Stop();

            // �N����~
            navMeshObstacle.enabled = false;
            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
            isFlame = false;
        }
        else if(isFlame==false)
        {
            flameSE.Play();

            // �N���J�n
            navMeshObstacle.enabled = true;
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
            isFlame = true;
        }
    }

    /// <summary>
    /// �M�~�b�N�N�����N�G�X�g
    /// </summary>
    void RequestActivateGimmick()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// �M�~�b�N�N������
    /// </summary>
    public override void TurnOnPower()
    {
        Ignition();
    }

    /// <summary>
    /// �M�~�b�N�ċN������
    /// </summary>
    public override void Reactivate()
    {
        if (timer > repeatRate) timer = repeatRate;
        InvokeRepeating("RequestActivateGimmick", repeatRate - timer, repeatRate);
    }
}
