//===================
// �����ŃX�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
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
            switch (gimmick.name)
            {
                case "SteelDoor_Set":
                    gimmick.GetComponent<SteelDoor>().TurnOnPower();
                    break;

                case "Belt Conveyor":

                    break;

                case "Fan":

                    break;

                case "PressMachine":
                    gimmick.GetComponent<PressMachine>().TurnOnPower();
                    break;

                case "SawBlade":
                    gimmick.GetComponent<SawBlade>().TurnOnPower();
                    break;

                default:
                    break;
            }
        }
    }
}
