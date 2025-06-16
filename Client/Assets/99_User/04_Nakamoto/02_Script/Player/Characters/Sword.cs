//--------------------------------------------------------------
// ���m�L���� [ SampleChara.cs ]
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
            isBlink = true;
            gameObject.layer = 21;
        }

        if (Input.GetKeyDown(KeyCode.X) && canAttack || Input.GetButtonDown("Attack1") && canAttack)
        {   // �U��1
            canAttack = false;
            animator.SetInteger("animation_id", (int)ANIM_ID.Attack);
            StartCoroutine(AttackCooldown());
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

    [ContextMenu("�V���b�N")]
    public void ShockTest()
    {
        this.GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
    }
}
