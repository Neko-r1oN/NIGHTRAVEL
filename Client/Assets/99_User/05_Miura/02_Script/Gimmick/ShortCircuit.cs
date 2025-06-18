using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerBase playerBase;
    EnemyBase enemyBase;
    Vector2 pos = Vector2.zero;

    /// <summary>
    /// �v���C���[�̃_���[�W�ʌv�Z����
    /// </summary>
    void HitPlayerDamage()
    {
        //�v���C���[�̍ő�HP����
        int maxHP = playerBase.MaxHP;

        //�_���[�W�ʂ͍ő�HP��5��
        int damage = Mathf.FloorToInt(maxHP * 0.05f);
        playerBase.ApplyDamage(damage);
    }

    /// <summary>
    /// �G�̃_���[�W�ʌv�Z����
    /// </summary>
    void HitEnemyDamage()
    {
        //�G�̍ő�HP����
        int maxLife = enemyBase.MaxHP;

        //�_���[�W�ʂ͍ő�HP��5��
        int damage = Mathf.FloorToInt(maxLife * 0.05f);
        enemyBase.ApplyDamage(damage);
    }

    /// <summary>
    /// �R�d�t�B�[���h�Ń_���[�W��^���鏈��
    /// </summary>
    /// <param name="collision">�Ԃ������I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            InvokeRepeating("HitPlayerDamage", 0.1f,0.5f);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            InvokeRepeating("HitEnemyDamage", 0.1f, 0.5f);
        }
    }

    /// <summary>
    /// �R�d�t�B�[���h�𗣂ꂽ���̏���
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CancelInvoke();
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            CancelInvoke();
        }
    }
}
