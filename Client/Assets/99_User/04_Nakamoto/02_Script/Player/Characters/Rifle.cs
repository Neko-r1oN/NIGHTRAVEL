//--------------------------------------------------------------
// ���C�t���L���� [ Rifle.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;   // HashSet �p
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
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
        BeamReady,
        SkillAfter
    }

    private bool isFiring = false;      // �r�[���Ǝ˒��t���O
    private bool isRailgun = false;     // �e�ό`�t���O

    [Foldout("�r�[���֘A")]
    [SerializeField] private Transform firePoint;           // ���˒n�_
    [Foldout("�r�[���֘A")]
    [SerializeField] private float maxDistance = 20f;       // �r�[���̒���
    [Foldout("�r�[���֘A")]
    [SerializeField] private float beamRadius = 0.15f;      // �r�[�����a
    [Foldout("�r�[���֘A")]
    [SerializeField] private float beamWidthScale = 1f;     // LineRenderer �ɏ�Z���Č����ڂ𒲐��������ꍇ
    [Foldout("�r�[���֘A")]
    [SerializeField] private LayerMask targetLayers;        // �G���C���[
    [Foldout("�r�[���֘A")]
    [SerializeField] private float duration = 2.5f;         // �Ǝˎ���
    [Foldout("�r�[���֘A")]
    [SerializeField] private float damageInterval = 0.3f;   // �_���[�W�Ԋu
    [Foldout("�r�[���֘A")]
    [SerializeField] private GameObject beamEffect;         // �r�[���G�t�F�N�g
    [Foldout("�r�[���֘A")]
    [SerializeField] private LineRenderer lr;

    [Foldout("�ʏ�U��")]
    [SerializeField] private float bulletSpeed;
    [Foldout("�ʏ�U��")]
    [SerializeField] private float bulletAccele;
    [Foldout("�ʏ�U��")]
    [SerializeField] private GameObject bulletPrefab;
    [Foldout("�ʏ�U��")]
    [SerializeField] private GameObject bulletSpawnObj;

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// ����t���O�����Z�b�g
    /// </summary>
    public override void ResetFlag()
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
            if (m_IsZipline) return;

            isSkill = true;
            canAttack = true;
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
        if (isSkill)
        {   // �e�ό`���͓����Ȃ��悤��
            m_Rigidbody2D.linearVelocity = new Vector2(0,m_Rigidbody2D.linearVelocityY);
            return;
        }

        base.Move(move, jump, blink);

        // �_�b�V�����̏ꍇ
        if (isBlinking)
        {   // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        // �e�ό`���̈ړ�����
        if (isRailgun)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocityY);
        }

        // ���ˎ����ɏ�����������
        if (isFiring)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 0.3f, m_Rigidbody2D.linearVelocityY);
        }
    }

    #endregion

    #region �U���E�_���[�W�֘A

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    public override void DoDashDamage()
    {

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
    /// �e�̔���
    /// </summary>
    public void FireBullet()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawnObj.transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetPlayer(m_Player);
        bullet.GetComponent<Bullet>().Speed = bulletSpeed;
        bullet.GetComponent<Bullet>().AcceleCoefficient = bulletAccele;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.localScale.x * bulletSpeed, 0);
    }

    #endregion

    #region ��_������

    public override void ApplyDamage(int power, Vector3? position = null, StatusEffectController.EFFECT_TYPE? type = null)
    {
        if (!invincible)
        {
            var damage = Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            UIManager.Instance.PopDamageUI(damage, transform.position, true);

            var id = animator.GetInteger("animation_id");
            if (position != null || id != 11 || id != 12) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);

            hp -= damage;
            Vector2 damageDir = Vector2.zero;

            // �m�b�N�o�b�N����
            if (position != null && id != 11 || position != null && id != 12)
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

    #region �r�[���֘A

    /// <summary>
    /// �X�L�����o�I����
    /// </summary>
    public void SkillEnd()
    {
        // ���ˏ�����
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.BeamReady);
    }

    /// <summary>
    /// ���ˏ�������
    /// </summary>
    public void ReadyToFire()
    {
        isSkill = false;
        isRailgun = true;
    }

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
        //lr.enabled = true;

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
            //lr.SetPosition(0, firePoint.position);
            //lr.SetPosition(1, endPos);
#endif

            yield return null;
        }

        // �r�[���G�t�F�N�g��\��
        beamEffect.SetActive(false);
        isFiring = false;
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.SkillAfter);
    }

    /// <summary>
    /// �X�L���I������
    /// </summary>
    public void EndSkill()
    {
        ResetFlag();
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    #endregion
}