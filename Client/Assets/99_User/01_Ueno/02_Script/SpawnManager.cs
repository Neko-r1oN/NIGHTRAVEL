using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {// ���ɂ�����
            // ����������
            this.GetComponent<SpriteRenderer>().enabled = true;
            
            // �v���C���[���X�g�Ƀv���C���[�̏����i�[
            this.GetComponent<EnemyBase>().enabled = true;
        }
    }
}
