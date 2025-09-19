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

    protected override void ApplyDamage()
    {
        if (isBroken == true) return;

        PlayerBase player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        //�j�ЃI�u�W�F�N�g�𐶐�(position.x�͔��̈ʒu�Ay�͔���菭����̈ʒu)
        GameObject fragment = Instantiate(BoxFragment, new Vector2(this.transform.position.x, this.transform.position.y + 1.5f), this.transform.rotation);

        for (int i = 0; i < fragment.transform.childCount; i++)
        {//fragment�̎q�̐��������[�v
            if (this.transform.position.x - player.transform.position.x >= 0)
            {//���̈ʒu�Ɣ�ׂāA�v���C���[�������ɂ�����
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200)); //�E���ɔj�Ђ��΂�
            }
            else
            {//���̈ʒu�Ɣ�ׂāA�v���C���[���E���ɂ�����
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200)); //�����ɔj�Ђ��΂�
            }
            FadeFragment(fragment.transform.GetChild(i));
        }

        //������
        Destroy(this.gameObject);
        GetComponent<AudioSource>().Play();
    }

    override public void DestroyFragment(Transform obj)
    {
        if (obj == null) return;
        Destroy(obj.gameObject);
    }

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    override public void FadeFragment(Transform fragment)
    {
        if (fragment == null) return;

        // 6�b�����Ĕj�Ђ��t�F�[�h�A�E�g�����A���̌�j�󂷂�
        fragment.GetComponent<Renderer>().material.DOFade(0, 6).OnComplete(() =>
        { DestroyFragment(fragment.transform);});
    }

    /// <summary>
    /// �{�b�N�X�j�󏈗�
    /// </summary>
    public override void TurnOnPower()
    {
        // �_���[�W�t�^
        ApplyDamage();
        isBroken = true; // �j��ς݂Ƃ���
    }

}
