//====================
//����Ƃ��锠�̃X�N���v�g
//Aouther:y-miura
//Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxFragment;�@//�j�ЃG�t�F�N�g���擾
    PlayerBase player;
    bool isBroken = false;

    public override void ApplyDamage()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();

        GameObject fragment; //�j�Ђ̃I�u�W�F�N�g
        fragment = Instantiate(BoxFragment, new Vector2(this.transform.position.x, this.transform.position.y - 1.5f), this.transform.rotation); //�j�ЃI�u�W�F�N�g�𐶐�(position.x�̓h�A�̈ʒu�Ay�̓h�A��菭�����̈ʒu)

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

        Destroy(this.gameObject);//������
        DestroyFragment(fragment.transform);
    }

    public override void FadeFragment(Transform fragment)
    {
        if (fragment == null)
        {
            return;
        }

        fragment.GetComponent<Renderer>().material.DOFade(0, 6);

    }

    public async void DestroyFragment(Transform fragment)
    {
        await Task.Delay(6000);

        if (fragment == null)
        {
            return;
        }

        Destroy(fragment.gameObject);
    }
}
