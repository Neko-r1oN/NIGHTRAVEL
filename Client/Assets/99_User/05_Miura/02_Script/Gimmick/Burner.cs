//===================
// �o�[�i�[�Ɋւ��鏈��
// Aouther:y-miura
// Date:2025/07/07
//===================

using System.Collections;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Burner : MonoBehaviour
{
    PlayerBase player;
    EnemyBase enemy;
    [SerializeField] GameObject flame;

    private void Start()
    {

    }

    private void Update()
    {

    }

    /// <summary>
    /// �G�ꂽ�I�u�W�F�N�g�ɉ�����ʂ�t�^���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = GetComponent<PlayerBase>();

            Debug.Log("�v���C���[�ɉ����Ԃ�t�^");
        }

        if (collision.CompareTag("Enemy"))
        {
            enemy = GetComponent<EnemyBase>();

            Debug.Log("�G�ɉ����Ԃ�t�^");
        }
    }

    /// <summary>
    /// ����_������������肷�鏈��
    /// </summary>
    private void Ignition()
    {

    }
}
