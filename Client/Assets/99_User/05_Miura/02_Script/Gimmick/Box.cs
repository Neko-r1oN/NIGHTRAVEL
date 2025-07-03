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
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾
    [SerializeField] GameObject BoxFragment;�@//�j�ЃG�t�F�N�g���擾
    PlayerBase player;

    bool isBroken = false;

    void Start()
    {
        //SpawnBox�֐����J��Ԃ��Ăяo���āA�����J��Ԃ���������
        InvokeRepeating("SpawnBox", 5.5f, 10);
    }

    /// <summary>
    /// ���𐶐����鏈��
    /// </summary>
    public void SpawnBox()
    {
        GameObject boxObj = BoxPrefab;

        //���𐶐�����
        Instantiate(boxObj, new Vector2(28.0f, 27.0f), Quaternion.identity);
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�G�ꂽ���̂̃^�O���uAbyss�v��������
        if(collision.CompareTag("Abyss"))
        {
            GameObject boxPrefab = BoxPrefab;

            //��������
            Destroy(boxPrefab);
        }
    }

    /// <summary>
    /// �����󂵂��Ƃ��̏���
    /// </summary>
    public override void ApplyDamage()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();

        GameObject fragment; //�j�Ђ̃I�u�W�F�N�g

        //�j�ЃI�u�W�F�N�g�𐶐�(position.x�͔��̈ʒu�Ay�͔���菭����̈ʒu)
        fragment = Instantiate(BoxFragment, new Vector2(this.transform.position.x, this.transform.position.y + 1.5f), this.transform.rotation);

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

        //�j�Ђ�����
        DestroyFragment(fragment.transform);
    }

    /// <summary>
    /// �j�Ђ��t�F�[�h�A�E�g�����鏈��
    /// </summary>
    /// <param name="fragment">�j�Ѓv���n�u</param>
    public override void FadeFragment(Transform fragment)
    {
        if (fragment == null)
        {
            //�������Ȃ�
            return;
        }

        //6�b�����Ĕj�Ђ��t�F�[�h�A�E�g������
        fragment.GetComponent<Renderer>().material.DOFade(0, 6);
    }

    /// <summary>
    /// �j�Ђ���������
    /// </summary>
    /// <param name="fragment">�j�Ѓv���n�u</param>
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
