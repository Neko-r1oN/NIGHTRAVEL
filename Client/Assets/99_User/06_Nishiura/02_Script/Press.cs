//===================
// �v���X�X�N���v�g
// Author:Nishiura
// Date:2025/07/07
//===================
using UnityEngine;

public class Press : MonoBehaviour
{
    PlayerBase playerBase;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �v���C���[���Ԃ��G���A�ɓ������ꍇ
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // �Ԃ���Ώۂ���PlayerBase���擾
            bool isGround =  playerBase.GetGrounded();  // PlayerBase����ڒn����ϐ����擾

            if (!isGround) return;  // �ڒn���Ă��Ȃ��ꍇ�A�������Ȃ�

            // �v���C���[�̍ő�HP20%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // �Ԃꂽ�v���C���[���`�F�b�N�|�C���g�֖߂�

            Debug.Log("You Pancaked");
        }
    }
}
