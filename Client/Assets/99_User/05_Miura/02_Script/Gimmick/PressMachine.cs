//====================
//�v���X�}�V���̃X�N���v�g
//Aouther:y-miura
//Date:2025/06/20
//====================

using DG.Tweening;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PressMachine : ObjectBase
{
    [SerializeField] GameObject machineFragment;
    PlayerBase player;
    bool isBroken = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.gameObject.transform.DOMoveY(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public override void ApplyDamage()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();

        GameObject fragment; //�j�Ђ̃I�u�W�F�N�g
        fragment = Instantiate(machineFragment, new Vector2(this.transform.position.x, this.transform.position.y - 1.5f), this.transform.rotation); //�j�ЃI�u�W�F�N�g�𐶐�(position.x�̓h�A�̈ʒu�Ay�̓h�A��菭�����̈ʒu)

        for (int i = 0; i < fragment.transform.childCount; i++)
        {//fragment�̎q�̐��������[�v
            if (this.transform.position.x - player.transform.position.x >= 0)
            {//�h�A�̈ʒu�Ɣ�ׂāA�v���C���[�������ɂ�����
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200)); //�E���ɔj�Ђ��΂�
            }
            else
            {//�h�A�̈ʒu�Ɣ�ׂāA�v���C���[���E���ɂ�����
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200)); //�����ɔj�Ђ��΂�
            }
            FadeFragment(fragment.transform.GetChild(i));
        }

        Destroy(this.gameObject);//�h�A����
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
        Destroy(fragment.gameObject);
    }
}
