//--------------------------------------------------------------
// ���m�L���� [ Sword.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using Pixeye.Unity;
using static StatusEffectController;
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

    #region �\�[�h��p�X�e�[�^�X

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillForth = 45f;        // �X�L���̈ړ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillTime = 0.5f;        // �X�L�����ʎ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillCoolDown = 5.0f;    // �X�L���̃N�[���_�E��

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float atkGravityCoefficient = 1.8f;   // �U�����̗������x�W��

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect1;   // �L�����ɔ�������G�t�F�N�g

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect2;   // ����ɔ�������G�t�F�N�g

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect3;   // �ǉ��Ŕ���������G�t�F�N�g

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

    #region �X�V�֘A����

    /// <summary>
    /// �X�V����
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || id == 3 || m_IsZipline) return;

            if (canAttack && !isCombo)
            {   // �U��1�i��
                canAttack = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // �U��2,3�i��
                if (id != (int)S_ANIM_ID.Attack3) isCombo = false;

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
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

        //-----------------------------
        // �f�o�b�O�p

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<StatusEffectController>().ApplyStatusEffect(EFFECT_TYPE.Burn);
        }

        //Esc�������ꂽ��
        if (Input.GetKey(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
        }
    }

    /// <summary>
    /// ����X�V����
    /// </summary>
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

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

        if (!canAttack)
        {
            // �U�����͗������x����
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, m_Rigidbody2D.linearVelocity.y / atkGravityCoefficient);
        }
    }

    #endregion

    #region �U���E�_���[�W�֘A

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    //public override void DoDashDamage()
    //{
    //    Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius,enemyLayer);

    //    for (int i = 0; i < collidersEnemies.Length; i++)
    //    {
    //        if (collidersEnemies[i].gameObject.tag == "Enemy")
    //        {
    //            //++ GetComponent��Enemy�X�N���v�g���擾���AApplyDamage���Ăяo���悤�ɕύX
    //            //++ �j��ł���I�u�W�F�����ۂɂ̓I�u�W�F�̋��ʔ�_���֐����ĂԂ悤�ɂ���

    //            collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(Power, playerPos);
    //            cam.GetComponent<CameraFollow>().ShakeCamera();
    //        }
    //        else if (collidersEnemies[i].gameObject.tag == "Object")
    //        {
    //            collidersEnemies[i].gameObject.GetComponent<ObjectBase>().ApplyDamage();
    //        }
    //    }
    //}

    public override void DoDashDamage()
    {
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);
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
                enemyComponent.ApplyDamage(Power, playerPos);
                processedEnemies.Add(enemyComponent); // �����ς݃��X�g�ɒǉ�

                // �J�����̃V�F�C�N����
                cam.GetComponent<CameraFollow>().ShakeCamera();
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
                    objectComponent.ApplyDamage();
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

        yield return new WaitForSeconds(skillCoolDown);
        canSkill = true;
    }

    #endregion

    #region ��_������

    public override void ApplyDamage(int power, Vector3? position = null, StatusEffectController.EFFECT_TYPE? type = null)
    {
        if (!invincible)
        {
            var damage = Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            UIManager.Instance.PopDamageUI(damage, transform.position, true);
            if (position != null) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;
            Vector2 damageDir = Vector2.zero;

            // �m�b�N�o�b�N����
            if (position != null)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * 40f;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_Rigidbody2D.AddForce(damageDir * 15);
            }

            if (type != null)
            {
                effectController.ApplyStatusEffect((StatusEffectController.EFFECT_TYPE)type);
            }

            if (hp <= 0)
            {   // ���S����
                m_Rigidbody2D.AddForce(damageDir * 10);
                StartCoroutine(WaitToDead());
            }
            else
            {   // ��_���d��
                if (position != null)
                {
                    StartCoroutine(Stun(0.35f));
                    StartCoroutine(MakeInvincible(0.4f));
                }
            }
        }
    }

    #endregion

    void OnDrawGizmos()
    {
        //�@CircleCast�̃��C������
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCheck.position, k_AttackRadius);
    }
}