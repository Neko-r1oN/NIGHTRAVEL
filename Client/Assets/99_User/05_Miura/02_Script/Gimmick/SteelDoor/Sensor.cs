//=========================================
//�|�S�������h�A�̊J�𔻒肷��Z���T�[�̃X�N���v�g
//Aouther:y-miura
//Date:2025/06/18
//=========================================
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    PlayerBase playerBase;
    SteelDoor steelDoor;

    /// <summary>
    /// �Z���T�[�͈͓��ɓ�������h�A���J���鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�v���C���[���|�S�������h�A�̃Z���T�[�͈͂ɓ�������
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            //playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoor�N���X��Open�֐����Ăяo��
            steelDoor.Open();
        }
    }

    /// <summary>
    /// �Z���T�[�͈͂��痣�ꂽ��h�A����鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�v���C���[���|�S�������h�A�̃Z���T�[�͈͂��痣�ꂽ��
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            //playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoor�N���X��Close�֐����Ăяo��
            steelDoor.Close();
        }
    }
}
