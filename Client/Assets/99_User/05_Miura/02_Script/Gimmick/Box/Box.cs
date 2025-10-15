//====================
// ����Ƃ��锠�̃X�N���v�g
// Aouther:y-miura
// Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using static Rewired.Glyphs.GlyphSet;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾
    [SerializeField] GameObject BoxFragment;�@//�j�ЃG�t�F�N�g���擾
    PlayerBase playerBase;
    EnemyBase enemyBase;

    // �j�󔻒�
    bool isBroken = false;

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�G�ꂽ���̂̃^�O���uAbyss�v��������
        if(collision.CompareTag("Gimmick/Abyss"))
        {//�X�e�[�W�O�ɏo����
            //��������
            Destroy(this.gameObject);
        }

        // �v���C���[���Ԃ��G���A�ɓ������ꍇ
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // �Ԃ���Ώۂ���PlayerBase���擾

            // �v���C���[�̍ő�HP20%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
        }

        // �G���Ԃ��G���A�ɓ������ꍇ
        if (collision.transform.tag == "Enemy")
        {
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();   // �Ԃ���Ώۂ���EnemyBase���擾

            // �G�ɑ�ʂ̃_���[�W��^���āA���������ɂ���
            int damage = 9999;

            if ((RoomModel.Instance && RoomModel.Instance.IsMaster) || !RoomModel.Instance)
            {
                enemyBase.ApplyDamageRequest(damage, null, false, false);
            }
        }
    }

    protected override void ApplyDamage()
    {

    }

    override public void DestroyFragment(Transform obj)
    {

    }

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    override public void FadeFragment(Transform fragment)
    {

    }

    /// <summary>
    /// �{�b�N�X�j�󏈗�
    /// </summary>
    public override void TurnOnPower()
    {
        isBroken = true; // �j��ς݂Ƃ���
    }
}
