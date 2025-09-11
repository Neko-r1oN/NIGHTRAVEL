//====================
// ����Ƃ��锠�̃X�N���v�g
// Aouther:y-miura
// Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾
    [SerializeField] GameObject BoxFragment;�@//�j�ЃG�t�F�N�g���擾

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
        {
            //��������
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// �d���I������
    /// </summary>
    public override void TurnOnPower()
    {
        // �_���[�W�t�^
        ApplyDamage(BoxFragment, isBroken, new Vector2(0f, 1.5f));
        isBroken = true; // �j��ς݂Ƃ���
    }

}
