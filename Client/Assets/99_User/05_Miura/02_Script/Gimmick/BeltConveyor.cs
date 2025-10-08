using UnityEngine;

public class BeltConveyor : MonoBehaviour
{
    //������̗͂ʂ̕ϐ�
    public float addPow;
    public float addPowEnemy;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        { // �Ώۂ��v���C���[�̏ꍇ
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPow, 0));
        }
       
        if(collision.CompareTag("Enemy"))
        { // �Ώۂ��G�̏ꍇ
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
       
        if (collision.CompareTag("Object"))
        { // �Ώۂ��I�u�W�F�N�g�̏ꍇ
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
    }
}
