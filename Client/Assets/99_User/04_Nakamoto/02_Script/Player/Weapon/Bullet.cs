//--------------------------------------------------------------
// �e�p���� [ Bullet.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Bullet : MonoBehaviour
{
    //--------------------------
    // �t�B�[���h

    private float timer;                    // �݌v��������
    private const float DEATH_TIME = 10f;   // �j������
    private PlayerBase player;              // ���˃L�����̏��

    /// <summary>
    /// �e�̑���
    /// </summary>
    public float Speed {  get; set; }

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= DEATH_TIME) Destroy(gameObject);
    }

    /// <summary>
    /// �v���C���[���擾
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayer(PlayerBase player)
    {
        this.player = player;
    }

    /// <summary>
    /// �e�̓����蔻��
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // �{�X���ǂ�������
            if (collision.gameObject.GetComponent<EnemyBase>().IsBoss)
            {
                // ���U���̒��I
                if (player.LotteryRelic(RELIC_TYPE.Rugrouter))
                {
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power / 2, player.gameObject, true, true, player.LotteryDebuff());
                }
                else
                {
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                }
            }
            else
            {
                // �{�X�̏ꍇ�̓{�X�G���A�ɓ����Ă��邩�m�F
                if (player.IsBossArea)
                {
                    // ���U���̒��I
                    if (player.LotteryRelic(RELIC_TYPE.Rugrouter))
                    {
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power / 2, player.gameObject, true, true, player.LotteryDebuff());
                    }
                    else
                    {
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                    }
                }
            }
        }
        else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Gimmick") || collision.gameObject.tag == "ground")
        {
            Destroy(gameObject);
        }
    }


}
