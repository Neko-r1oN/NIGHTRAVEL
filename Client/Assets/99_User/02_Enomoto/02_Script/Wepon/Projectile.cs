//**************************************************
//  ���˕��̃N���X
//  Author:r-enomoto
//**************************************************
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Vector2 direction;
    [SerializeField] float speed = 15f;
    GameObject owner;
    bool hasHit = false;

    public void Initialize(Vector2 direction, GameObject owner)
    {
        this.direction = direction;
        this.owner = owner;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);  // �������ŏ�����𐳖ʂɂ��Ă���
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!hasHit)
            GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;    // ������𐳖ʂɂ��Ă��邽��up���w��
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(2, transform.position);
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag != "EnemyBase" && collision.gameObject.tag != "Player" && collision.gameObject != owner)
        {
            Destroy(gameObject);
        }

    }
}
