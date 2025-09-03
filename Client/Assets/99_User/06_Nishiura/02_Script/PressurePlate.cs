//===================
// �����ŃX�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    // ��������ϐ�
    private bool isPushed = false;

    // �֘A�t����ꂽ�M�~�b�N���X�g
    [SerializeField] List<GameObject> linkedGimmick = new List<GameObject>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �v���C���[���G�ꂽ�ꍇ�������ꂽ��ԂłȂ��ꍇ
        if (collision.transform.tag == "Player" && isPushed != true)
        {
            isPushed = true; 
            this.transform.DOMoveY((this.gameObject.transform.position.y -0.19f), 0.2f);
            PowerOn();
            Debug.Log("Plate Activated");
        }
    }

    /// <summary>
    /// �d���I������
    /// </summary>
    private void PowerOn()
    {
        foreach (GameObject gimmick in linkedGimmick)
        {
            switch (gimmick.tag)
            {
                case "Gimmick/SteelDoor":
                    gimmick.GetComponent<SteelDoor>().TurnOnPower(1);
                    break;

                case "Gimmick/BeltConveyor":

                    break;

                case "Gimmick/Fan":

                    break;

                case "Gimmick/PressMachine":
                    gimmick.GetComponent<PressMachine>().TurnOnPower(1);
                    break;

                case "Gimmick/SawBlade":
                    gimmick.GetComponent<SawBlade>().TurnOnPower(1);
                    break;

                case "Gimmick/Elevator":
                    gimmick.GetComponent<Elevator>().TurnOnPower(1);
                    break;

                default:
                    break;
            }
        }
    }
}
