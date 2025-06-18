using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    PlayerBase playerBase;
    SteelDoor steelDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�v���C���[���|�S���h�A�̃Z���T�[�ɐG�ꂽ��
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoor�N���X��Up�֐����Ăяo��(�h�A����Ɉړ�����)
            steelDoor.Up();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//�v���C���[���|�S���h�A�̃Z���T�[���痣�ꂽ��
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoor�N���X��Down�֐����Ăяo��(�h�A�̈ʒu�����Ƃɖ߂�)
            steelDoor.Down();
        }
    }
}
