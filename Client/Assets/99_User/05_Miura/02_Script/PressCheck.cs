//=========================================
// �G���x�[�^�[�ŋ��܂�ڐG����I�u�W�F�N�g�̃X�N���v�g
// Aouther:y-miura
// Date:2025/09/25
//=========================================

using UnityEngine;

public class PressCheck : MonoBehaviour
{
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
        }
        if(collision.gameObject.CompareTag("Enemy"))
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
