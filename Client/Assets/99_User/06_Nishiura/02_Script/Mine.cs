//===================
// �n���X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Mine : MonoBehaviour
{
    [SerializeField] GameObject boomEffect; // �����G�t�F�N�g�v���n�u

    PlayerBase playerBase;
    EnemyBase enemyBase;

    Vector2 pos;

    private void Start()
    {
        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.gameObject.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Instantiate(boomEffect);    // �����G�t�F�N�g�𐶐�
            Destroy(this.gameObject);   // ���g��j��
            Debug.Log("Boomed Mine");
        }else if (collision.transform.tag == "Enemy")
        {
            Destroy(this.gameObject);
            Debug.Log("Boomed Mine");
        }
    }
}
