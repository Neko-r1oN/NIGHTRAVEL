//===================
// �\�[�u���[�h�X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    PlayerBase playerBase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // �v���C���[�̍ő�HP15%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.15f);
            playerBase.ApplyDamage(damage, this.gameObject.transform.position);
            Debug.Log("Hit SawBlade");
        }
    }

    private void MoveBlade()
    {
        //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        //Append�œ����ǉ����Ă���
        sequence.Append(this.transform.DOMoveY(-1, 1))
                .AppendInterval(1)
                .Append(this.transform.DOMoveY(1, 2));

        //Play�Ŏ��s
        sequence.Play()
                .AppendInterval(1)
                .SetLoops(-1);
    }
}
