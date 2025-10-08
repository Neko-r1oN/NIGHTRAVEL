using UnityEngine;

public class BeltConveyor : MonoBehaviour
{
    //������̗͂ʂ̕ϐ�
    public float addPow;
    public float addPowEnemy;
    private void OnTriggerStay2D(Collider2D collision)
    {
        // �Ώۂ��v���C���[�̏ꍇ
        if(collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            //�Ԃ������I�u�W�F�N�g�ɁAaddPow���̗͂�������
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPow, 0));
        }
        // �Ώۂ��G�̏ꍇ
        if(collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }

        if (collision.CompareTag("Object"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
    }
}
