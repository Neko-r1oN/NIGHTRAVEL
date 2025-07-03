//===================
// �����G�t�F�N�g�X�N���v�g
// Author:Nishiura
// Date:2025/07/03
//===================
using UnityEngine;

public class BoomEffect : MonoBehaviour
{
    PlayerBase playerBase;
    Vector2 pos;

    private void Start()
    {
        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.gameObject.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // �v���C���[�̍ő�HP30%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.30f);
            playerBase.ApplyDamage(damage, pos);

            Invoke("DeleteThis", 0.8f);
        }
    }

    /// <summary>
    /// �G�t�F�N�g��������
    /// </summary>
    private void DeleteThis()
    {
        Destroy(this.gameObject);
    }
}
