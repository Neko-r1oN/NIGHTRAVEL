//========================
//����I�u�W�F�N�g�̐e�N���X
//Author:y-Miura
//date:2025/03/17
//========================

using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.UIElements;

abstract public class ObjectBase : GimmickBase
{
    /// <summary>
    /// ����I�u�W�F�N�g�̃_���[�W�֐�
    /// </summary>
    virtual protected void ApplyDamage(GameObject fragmentObj, bool isBroken, Vector2 pos)
    {
        if (isBroken == true) return;

        PlayerBase player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        //�j�ЃI�u�W�F�N�g�𐶐�(position.x�͔��̈ʒu�Ay�͔���菭����̈ʒu)
        GameObject fragment = Instantiate(fragmentObj, new Vector2(this.transform.position.x, this.transform.position.y + pos.y), this.transform.rotation);

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
    }

    virtual protected void DestroyFragment(Transform obj)
    {
        if (obj == null) return;
        Destroy(obj.gameObject);
    }

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    virtual protected void FadeFragment(Transform fragment)
    {
        if (fragment == null) return;

        // 6�b�����Ĕj�Ђ��t�F�[�h�A�E�g�����A���̌�j�󂷂�
        fragment.GetComponent<Renderer>().material.DOFade(0, 6).OnComplete(() =>
        { DestroyFragment(fragment.transform); });
    }
}
