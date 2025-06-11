using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Player player;
    EnemyController enemyController;
    Vector2 pos = Vector2.zero;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayerHitDamage()
    {
        //int maxLife = (int)player.HP; //�v���C���[�̍ő�HP

        ////�v���C���[�̃_���[�W�v�Z
        //int damage = Mathf.FloorToInt(maxLife * 0.05f); //�v�_���[�W�ʂ�float����int�ɕϊ�
        //player.DealDamage(this.gameObject, damage, Vector2.zero); //�g���b�v���ƂɑΉ������_���[�W�Ή��������Ăяo��
    }

    void EnemyHitDamage()
    {
        int HP = enemyController.maxHP; //�G�̍ő�HP

        //==========
        //�_���[�W�v�Z
        //==========
        int damage = Mathf.FloorToInt(HP * 0.05f); //�_���[�W�ʂ�
        enemyController.ApplyDamage(damage); //�G�̃_���[�W�Ή��֐����Ăяo��
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {//�v���C���[/�G���R�d�t�B�[���h�ɓ���������
        if (collision.gameObject.CompareTag("Player"))
        {//�v���C���[���R�d�t�B�[���h�ɓ���������
            player = collision.GetComponent<Player>(); //player�ɁA�����������̂���
            InvokeRepeating("PlayerHitDamage", 0.1f, 0.5f); //�uPlayerHitDamage�v���A0.1�b�ԁA0.5�b���ƂɌJ��Ԃ�
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            enemyController = collision.GetComponent<EnemyController>(); //enemyController�ɁA�����������̂���
            InvokeRepeating("EnemyHitDamage", 0.1f, 0.5f); //�uEnemyHitDamage�v���A0.1�b�ԁA0.5�b���ƂɌJ��Ԃ�
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {//�v���C���[/�G���R�d�t�B�[���h���痣�ꂽ��
        if (collision.gameObject.CompareTag("Player"))
        {//�v���C���[���R�d�t�B�[���h���痣�ꂽ��
            player = collision.GetComponent<Player>(); //player�ɁA�����������̂���
            CancelInvoke(); //�uPlayerHitDmage�v�̃��s�[�g���~�߂�
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//�G���R�d�t�B�[���h���痣�ꂽ��
            enemyController = collision.GetComponent<EnemyController>(); //enemyController�ɁA�����������̂���
            CancelInvoke(); //�uEnemyHitDamage�v�̃��s�[�g���~�߂�
        }
    }
}
