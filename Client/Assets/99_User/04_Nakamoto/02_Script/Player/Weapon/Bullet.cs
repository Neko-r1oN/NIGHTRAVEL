//--------------------------------------------------------------
// �e�p���� [ Bullet.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //--------------------------
    // �t�B�[���h

    private float timer;                    // �݌v��������
    private bool orbitFlag;                 // �ǔ��t���O
    private PlayerBase player;              // ���˃L�����̏��
    [SerializeField] private float trackingStart;           // �ǔ��J�n����

    /// <summary>
    /// �e�̑���
    /// </summary>
    public float Speed {  get; set; }

    /// <summary>
    /// �����W��
    /// </summary>
    public float AcceleCoefficient { get; set; }

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;

        if(trackingStart <= timer && !orbitFlag)
        {   // �O���ω�
            orbitFlag = true;

            var target = FetchNearObjectWithTag("Enemy");

            // �G�Ɍ����ĉ���
            if(target != null)
            {
                var vec = target.transform.position - transform.position;
                vec = vec.normalized;
                gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec);
                gameObject.GetComponent<Rigidbody2D>().linearVelocity = (vec * Speed) * AcceleCoefficient;
            }
        }

        if(timer >= 10f) Destroy(gameObject);
    }

    /// <summary>
    /// �v���C���[���̎擾
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
            collision.gameObject.GetComponent<EnemyBase>().ApplyDamage(player.Power, player.transform);
        }
        else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }else if(collision.gameObject.tag == "Default" || collision.gameObject.tag == "ground")
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �P�ԋ߂��I�u�W�F�N�g���擾����
    /// </summary>
    /// <param name="tagName">�擾������tagName</param>
    /// <returns>�ŏ������̎w��Obj</returns>
    public Transform FetchNearObjectWithTag(string tagName)
    {
        // �Y���^�O��1���������ꍇ�͂����Ԃ�
        var targets = GameObject.FindGameObjectsWithTag(tagName);
        if (targets.Length == 1) return targets[0].transform;

        GameObject result = null;               // �Ԃ�l
        var minTargetDistance = float.MaxValue; // �ŏ�����
        foreach (var target in targets)
        {
            // �O��v�������I�u�W�F�N�g�����߂��ɂ���΋L�^
            var targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (!(targetDistance < minTargetDistance)) continue;
            minTargetDistance = targetDistance;
            result = target.transform.gameObject;
        }

        // �Ō�ɋL�^���ꂽ�I�u�W�F�N�g��Ԃ�
        return result?.transform;
    }
}
