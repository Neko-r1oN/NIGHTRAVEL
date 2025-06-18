//--------------------------------------------------------------
// ���m�L���� [ Sword.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using Pixeye.Unity;
using System;
using HardLight2DUtil;

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
        Skill
    }

    private bool isCombo = false;

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        // �L�����̈ړ�
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;
        Ladder();

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // �W�����v������
            if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
        {   // �u�����N������
            if(canAttack)
            {
                isBlink = true;
                gameObject.layer = 21;
            }
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            Debug.Log("�U���F" + canAttack + " �R���{�F" + isCombo);
            if (canAttack && !isCombo)
            {
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {
                isCombo = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
            }

            canAttack = false;
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �U��2
            GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
        }

        //-----------------------------
        // �f�o�b�O�p

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetExp(testExp);
            Debug.Log("�l���o���l�F" + testExp + "�����x���F" + nowLv + " ���o���l�F" + nowExp + "�K�v�o���l" + nextLvExp);
        }
    }

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

                collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(power, playerPos);
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
    public void HitAttack()
    {
        isCombo = true;
    }

    /// <summary>
    /// �U���I����
    /// </summary>
    public void AttackEnd()
    {
        canAttack = true;
        isCombo = false;

        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    [ContextMenu("�V���b�N")]
    public void ShockTest()
    {
        this.GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
    }
}
