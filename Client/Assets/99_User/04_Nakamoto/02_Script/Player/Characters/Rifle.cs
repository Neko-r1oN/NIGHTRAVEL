using UnityEngine;
using static StatusEffectController;
using static Sword;

public class Rifle : PlayerBase
{
    /// <summary>
    /// �X�V����
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��

        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �U��2

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
}
