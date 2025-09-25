//===================
// �v���X�X�N���v�g
// Author:Nishiura
// Date:2025/07/07
//===================
using UnityEngine;

public class Press : MonoBehaviour
{
    PlayerBase playerBase;

    [SerializeField] PressCheck pressUP;
    [SerializeField] PressCheck pressDown;

    private void OnTriggerStay2D(Collider2D collision)
    {
        bool isPressUP = pressUP.gameObject.GetComponent<PressCheck>().isToutch;
        bool isPressDown = pressDown.gameObject.GetComponent<PressCheck>().isToutch;

        // �v���C���[���Ԃ��G���A�ɓ������ꍇ
        if (collision.gameObject.CompareTag("Player") && isPressUP == true || isPressDown==true)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // �Ԃ���Ώۂ���PlayerBase���擾                         

            // �v���C���[�̍ő�HP70%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.7f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // �Ԃꂽ�v���C���[���`�F�b�N�|�C���g�֖߂�

            Debug.Log("You Pancaked");
        }
    }
}
