//==================
// �����ŃX�N���v�g
// Author:Nishiura
// Date:2025/07/01
//==================
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    // ��������ϐ�
    private bool isPushed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �v���C���[���G�ꂽ�ꍇ�������ꂽ��ԂłȂ��ꍇ
        if (collision.transform.tag == "Player" && isPushed != true)
        {
            isPushed = true;
            Debug.Log("Plate Activated");
        }
    }
}
