//=========================================
// �G���x�[�^�[�ŋ��܂�ڐG����I�u�W�F�N�g�̃X�N���v�g
// Aouther:y-miura
// Date:2025/09/25
//=========================================

using UnityEngine;

public class PressCheck : MonoBehaviour
{
    Press press;
    PlayerBase playerBase;
    public bool isToutch = false;

    public bool ObjectPressCheck() { return isToutch; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    /// <summary>
    /// �V��A�n�ʂɐG�ꂽ���̏���
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//�v���C���[���G�ꂽ��
            isToutch = true; //�G�ꂽ���Ƃɂ���

            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // �Ԃ���Ώۂ���PlayerBase���擾

            if (!isToutch) return;  // �ڒn���Ă��Ȃ��ꍇ�A�������Ȃ�

            // �v���C���[�̍ő�HP20%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // �Ԃꂽ�v���C���[���`�F�b�N�|�C���g�֖߂�

        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//�G���G�ꂽ��
            isToutch = true; //�G�ꂽ���Ƃɂ���
        }
    }

    /// <summary>
    /// �V��/�����痣�ꂽ�Ƃ��̏���
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//�v���C���[�����ꂽ��
            isToutch = false;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//�G�����ꂽ��
            isToutch = true;
        }
    }
}
