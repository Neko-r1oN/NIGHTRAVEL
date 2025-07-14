using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;   // HashSet �p
using UnityEngine;
using static StatusEffectController;
using static Sword;

public class Rifle : PlayerBase
{
    //--------------------------
    // �t�B�[���h

    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum GS_ANIM_ID
    {
        Attack = 10,
        Skill,
        BeamReady
    }

    private bool isFiring = false;      // �r�[���Ǝ˒��t���O
    private bool isRailgun = false;     // �e�ό`�t���O

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private Transform firePoint;           // ���˒n�_
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float maxDistance = 20f;       // �r�[���̒���
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float beamRadius = 0.15f;      // �r�[�����a
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float beamWidthScale = 1f;     // LineRenderer �ɏ�Z���Č����ڂ𒲐��������ꍇ
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private LayerMask targetLayers;        // �G���C���[
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float duration = 2.5f;         // �Ǝˎ���
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float damageInterval = 0.3f;   // �_���[�W�Ԋu
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private GameObject beamEffect;         // �r�[���G�t�F�N�g
    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private LineRenderer lr;

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// ��_�����e�t���O�����Z�b�g
    /// </summary>
    public override void HitReset()
    {
        canAttack = true;
        isFiring = false;
        isRailgun = false;
        isSkill = false;
    }

    #region �X�V�֘A����

    /// <summary>
    /// �X�V����
    /// </summary>
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || id == 3 || m_IsZipline) return;

            if (canAttack)
            {
                if (isRailgun)
                {
                    canAttack = false;
                    isRailgun = false;
                    FireLaser(new Vector2(transform.localScale.x,0));
                }
                else
                {
                    canAttack = false;
                    animator.SetInteger("animation_id", (int)GS_ANIM_ID.Attack);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �X�L��
            isSkill = true;
            animator.SetInteger("animation_id", (int)GS_ANIM_ID.Skill);
        }

        base.Update();

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
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="blink">�_�b�V������</param>
    protected override void Move(float move, bool jump, bool blink)
    {
        if (isSkill || isRailgun)
        {   // �e�ό`���͓����Ȃ��悤��
            m_Rigidbody2D.linearVelocity = Vector3.zero;
            return;
        }

        base.Move(move, jump, blink);

        // �_�b�V�����̏ꍇ
        if (isBlinking)
        {   // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        // �e�ό`���̈ړ�����
        if (isFiring)
        {
            m_Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    #endregion

    #region �U���E�_���[�W�֘A

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    public override void DoDashDamage()
    {
        power = Mathf.Abs(power);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    power = -power;
                }
                //++ GetComponent��Enemy�X�N���v�g���擾���AApplyDamage���Ăяo���悤�ɕύX
                //++ �j��ł���I�u�W�F�����ۂɂ̓I�u�W�F�̋��ʔ�_���֐����ĂԂ悤�ɂ���

                collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(1, playerPos);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
            else if (collidersEnemies[i].gameObject.tag == "Object")
            {
                collidersEnemies[i].gameObject.GetComponent<ObjectBase>().ApplyDamage();
            }
        }
    }

    /// <summary>
    /// �U���I����
    /// </summary>
    public void AttackEnd()
    {
        canAttack = true;

        // Idle�ɖ߂�
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// �X�L�����o�I����
    /// </summary>
    public void SkillEnd()
    {
        isSkill = false;
        isRailgun = true;

        // ���ˏ�����
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.BeamReady);
    }

    #endregion

    #region �r�[���֘A

    /// <summary>
    /// �Ǝˏ���
    /// </summary>
    /// <param name="direction">�v���C���[�̌���</param>
    public void FireLaser(Vector2 direction)
    {
        if (isFiring) return;             // ���d���˖h�~
        StartCoroutine(LaserRoutine(direction.normalized));
    }

    /// <summary>
    /// �Ǝ˒�����
    /// </summary>
    /// <param name="dir">����</param>
    /// <returns></returns>
    private IEnumerator LaserRoutine(Vector2 dir)
    {
        isFiring = true;

        // �r�[���G�t�F�N�g�\��
        beamEffect.SetActive(true);

        float laserTimer = 0f;   // �S�̂̏Ǝˎ���
        float tickTimer = 0f;    // �_���[�W�Ԋu�v��

#if UNITY_EDITOR
        lr.enabled = true;

        // LineRenderer �̑����𓖂��蔻��ƍ��킹��
        float lrWidth = beamRadius * 2f * beamWidthScale;
        lr.startWidth = lrWidth;
        lr.endWidth = lrWidth;
#endif

        while (laserTimer < duration)
        {
            laserTimer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            // CircleCastAll �Łu�����v��������������
            RaycastHit2D[] hits = Physics2D.CircleCastAll(
                origin: firePoint.position,
                radius: beamRadius,
                direction: dir,
                distance: maxDistance,
                layerMask: targetLayers);

            // ���[�U�[�����F�ŏ��ɏՓ˂����ʒu or �ő勗��
            Vector3 endPos = firePoint.position + (Vector3)(dir * maxDistance);
            if (hits.Length > 0) endPos = hits[0].point;

            // �w��_���[�W�Ԋu���Ƀ_���[�W
            if (tickTimer >= damageInterval && hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == null) continue;

                    hit.collider.gameObject.GetComponent<EnemyBase>().ApplyDamage(this.Power, this.transform);
                }
                tickTimer = 0f;
            }

#if UNITY_EDITOR
            // LineRenderer �X�V
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, endPos);
#endif

            yield return null;
        }

        // �r�[���G�t�F�N�g��\��
        beamEffect.SetActive(false);
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
        isFiring = false;
        canAttack = true;
    }

    #endregion
}