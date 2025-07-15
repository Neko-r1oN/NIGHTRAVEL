//--------------------------------------------------------------
// �e�p���� [ Bullet.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //--------------------------
    // �t�B�[���h

    private float timer;                    // �݌v��������
    private bool orbitFlag;                 // �ǔ��t���O
    private PlayerBase player;              // ���˃L�����̏��
    [SerializeField] private float trackingStart;            // �ǔ��J�n����

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

            var target = player.FetchNearObjectWithTag("Enemy");

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
            Destroy(gameObject);
        }else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }
    }
}
