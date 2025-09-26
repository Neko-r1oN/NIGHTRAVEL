//--------------------------------------------------------------
// ���m�L���� [ Sword.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using Pixeye.Unity;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Sword : PlayerBase
{
    //--------------------------
    // �t�B�[���h

    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum S_ANIM_ID
    {
        Attack1 = 10,
        Attack2,
        Attack3,
        Skill
    }
    private bool isCombo = false;       // �R���{�\�t���O
    private float plDirection = 0;      // �v���C���[�̌���

    #region ���m��p�X�e�[�^�X

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillForth = 45f;        // �X�L���̈ړ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillTime = 0.5f;        // �X�L�����ʎ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillCoolDown = 5.0f;    // �X�L���̃N�[���_�E��

    [Foldout("�A�^�b�N�G�t�F�N�g")]
    [SerializeField] private ParticleSystem normalEffect1;   // �ʏ�U��1

    [Foldout("�A�^�b�N�G�t�F�N�g")]
    [SerializeField] private ParticleSystem normalEffect2;   // �ʏ�U��2

    [Foldout("�A�^�b�N�G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect1;   // �L�����ɔ�������G�t�F�N�g

    [Foldout("�A�^�b�N�G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect2;   // ����ɔ�������G�t�F�N�g

    [Foldout("�A�^�b�N�G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect3;   // �ǉ��Ŕ���������G�t�F�N�g

    private bool isAirAttack = false;   // �󒆍U�����������ǂ���

    // �e�U���̍U���͔{��
    private const float ATTACK2_MAG = 1.1f;
    private const float ATTACK3_MAG = 1.3f;
    private const float SKILL_MAG = 1.65f;
    #endregion

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// ����t���O�����Z�b�g
    /// </summary>
    public override void ResetFlag()
    {
        canAttack = true;
        isCombo = false;
        isSkill = false;
    }

    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        playerType = Player_Type.Sword;
    }

    #region �X�V�֘A����

    /// <summary>
    /// �X�V����
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // �L�����̈ړ�
        if (!canMove) return;

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || id == (int)ANIM_ID.Hit || m_IsZipline || isAirAttack) return;

            if (canAttack && !isCombo)
            {   // �U��1�i��
                canAttack = false;
                normalEffect1.Play();
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // �U��2,3�i��
                if (id != (int)S_ANIM_ID.Attack3) isCombo = false;

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    normalEffect2.Play();
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                }
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Attack2"))
        {   // �U��2
            if (canSkill && canAttack)
            {
                //gameObject.layer = 21;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Skill);
                canSkill = false;
                plDirection = transform.localScale.x;
                StartCoroutine(SkillCoolDown());
            }
        }
    }

    /// <summary>
    /// ����X�V����
    /// </summary>
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(m_Grounded) isAirAttack = false;

        if (isSkill)
        {
            // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(plDirection * skillForth, 0);
        }
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="blink">�_�b�V������</param>
    protected override void Move(float move, bool jump, bool blink)
    {
        base.Move(move, jump, blink);

        // �_�b�V�����̏ꍇ
        if (isBlinking)
        {   // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        if (!m_Grounded && !canAttack)
        {
            // �U�����͗������x����
            m_Rigidbody2D.gravityScale = 0;
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
        }
        else
        {
            m_Rigidbody2D.gravityScale = gravity;
        }
    }

    void OnDrawGizmos()
    {
        //�@CircleCast�̃��C������
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCheck.position, ATTACK_RADIUS);
    }

    #endregion

    #region �U���E�_���[�W�֘A
    public override void DoDashDamage()
    {
        // �U���͈͂ɋ���G���擾
        int animID = animator.GetInteger("animation_id");
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, ATTACK_RADIUS);
        HashSet<EnemyBase> processedEnemies = new HashSet<EnemyBase>();

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if(collidersEnemies[i].gameObject.tag == "Enemy")
            {
                var enemyComponent = collidersEnemies[i].gameObject.GetComponent<EnemyBase>();
                if (enemyComponent == null) continue; // EnemyBase���t���Ă��Ȃ��I�u�W�F�N�g���X�L�b�v

                // ���Ƀ_���[�W�����ς݂̓G���ǂ����`�F�b�N
                if (processedEnemies.Contains(enemyComponent))
                    continue;

                // �G�Ƀ_���[�W��^����
                int attackPower = 0;    // �ŏI�U����

                // �U���͂�ݒ�
                switch (animID)
                {
                    case (int)S_ANIM_ID.Attack1:
                        attackPower = Power;
                        break;

                    case (int)S_ANIM_ID.Attack2:
                        attackPower = (int)(Power * ATTACK2_MAG);
                        break;

                    case (int)S_ANIM_ID.Attack3:
                        attackPower = (int)(Power * ATTACK3_MAG);
                        break;

                    case (int)S_ANIM_ID.Skill:
                        attackPower = (int)(Power * SKILL_MAG);
                        break;

                    default:
                        attackPower = Power;
                        break;
                }

                // �{�X���ǂ�������
                if(!collidersEnemies[i].gameObject.GetComponent<EnemyBase>().IsBoss)
                {
                    // ���U���̒��I
                    if (LotteryRelic(RELIC_TYPE.Rugrouter))
                    {
                        enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                        enemyComponent.ApplyDamageRequest(attackPower / 2, gameObject, true, true, LotteryDebuff());
                    }
                    else
                    {
                        enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                    }
                }
                else
                {
                    // �{�X�̏ꍇ�̓{�X�G���A�ɓ����Ă��邩�m�F
                    if(isBossArea)
                    {
                        // ���U���̒��I
                        if (LotteryRelic(RELIC_TYPE.Rugrouter))
                        {
                            enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                            enemyComponent.ApplyDamageRequest(attackPower / 2, gameObject, true, true, LotteryDebuff());
                        }
                        else
                        {
                            enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                        }
                    }
                }

                    processedEnemies.Add(enemyComponent); // �����ς݃��X�g�ɒǉ�
            }
        }

        // Object�^�O�̏���
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Object")
            {
                var objectComponent = collidersEnemies[i].gameObject.GetComponent<ObjectBase>();
                if (objectComponent != null)
                {
                    objectComponent.TurnOnPowerRequest(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// �R���{����J�n
    /// </summary>
    public void HitAttack()
    {
        isCombo = true;
    }

    /// <summary>
    /// �U���I����
    /// </summary>
    public void AttackEnd()
    {
        if (!isCombo) return;

        // �t���O�̏�����
        canAttack = true;
        isCombo = false;

        if (!m_Grounded) isAirAttack = true;

        // Idle�ɖ߂�
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// �X�L���N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillCoolDown()
    {
        isSkill = true;

        // Effect�Đ�
        skillEffect1.SetActive(true);
        skillEffect2.SetActive(true);
        skillEffect3.SetActive(true);

        yield return new WaitForSeconds(skillTime);
        isSkill = false;
        gameObject.layer = 20;

        // Effect��\��
        skillEffect1.SetActive(false);
        skillEffect2.SetActive(false);
        skillEffect3.SetActive(false);

        // �ړ����x�ɉ����ăA�j���[�V��������
        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        // �����b�N�u�}�E�X�v�������̓N�[���_�E������
        if (LotteryRelic(RELIC_TYPE.Mouse))
        {
            canSkill = true;
            yield break;
        }

        UIManager.Instance.DisplayCoolDown(true, skillCoolDown);
        yield return new WaitForSeconds(skillCoolDown);
        canSkill = true;
    }

    #endregion

    #region ��_������

    public override void ApplyDamage(int power, Vector3? position = null, KB_POW? kbPow = null, DEBUFF_TYPE? type = null)
    {
        // ���G�ȊO�̎�
        if (!invincible)
        {
            // �����񕜒�~
            StartCoroutine(RegeneStop());

            // �_���[�W�v�Z
            int damage = (type == null) ? Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense)): power;
            damage = (kbPow == null) ? power : Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            // �_���[�W�\�L
            UIManager.Instance.PopDamageUI(damage, transform.position, true);

            // �A�j���[�V�����ύX
            var id = animator.GetInteger("animation_id");
            if (position != null && id != (int)S_ANIM_ID.Skill) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);

            // ��𔻒�
            if (LotteryRelic(RELIC_TYPE.HolographicArmor))
            {
                // ��𐬌��\��
            }
            else
            {
                // HP����
                hp -= damage - (int)(damage * firewallRate);
            }

            Vector2 damageDir = Vector2.zero;

            // �m�b�N�o�b�N����
            if (position != null)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * KNOCKBACK_DIR;
                m_Rigidbody2D.linearVelocity = Vector2.zero;

                // �����ɉ����ăm�b�N�o�b�N�͂�ύX
                switch (kbPow)
                {
                    case KB_POW.Small:
                        m_Rigidbody2D.AddForce(damageDir * KB_SMALL);
                        break;

                    case KB_POW.Medium:
                        m_Rigidbody2D.AddForce(damageDir * KB_MEDIUM);
                        break;

                    case KB_POW.Big:
                        m_Rigidbody2D.AddForce(damageDir * KB_BIG);
                        break;

                    default:
                        break;
                }
            }

            // ��Ԉُ�t�^
            if (type != null)
            {
                effectController.ApplyStatusEffect((DEBUFF_TYPE)type);
            }

            if (hp <= 0)
            {   // ���S����
                hp = 0;
                canMove = false;
                isRegene = false;
                m_Rigidbody2D.AddForce(damageDir * KB_MEDIUM);
                StartCoroutine(WaitToDead());
            }
            else
            {   // ��_���d��
                if (position != null)
                {
                    StartCoroutine(Stun(STUN_TIME));
                    StartCoroutine(MakeInvincible(INVINCIBLE_TIME));
                }
            }
        }
    }

    #endregion
}