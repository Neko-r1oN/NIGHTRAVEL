using UnityEngine;
using System.Collections;
using UnityEngine.UI; // UI�R���|�[�l���g�̎g�p
using UnityEngine.InputSystem;

public class menu : MonoBehaviour
{
    Button cube;
    Button sphere;
    Button cylinder;

    private Gamepad gamepad;
    private bool isConnected;
    private bool isClick;

    void Start()
    {
        gamepad = Gamepad.current;
        isClick = false;

        // �{�^���R���|�[�l���g�̎擾
        cube = GameObject.Find("/Canvas/Buttons/Button(Solo)").GetComponent<Button>();

        // �ŏ��ɑI����Ԃɂ������{�^���̐ݒ�
        cube.Select();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isClick = true;
        }

        if (isClick || isConnected)
        {

             // ���X�e�B�b�N�̓��͒l���擾
             Vector2 stickInput = gamepad.leftStick.ReadValue();

             if (stickInput.x != 0 || stickInput.y != 0)
             {
                 if (isClick)
                 {
                     cube.Select();
                     isClick = false;
                 }
             }
        }

        if (gamepad == null)
        {
            isConnected = false;
        }
        else if (gamepad != null)
        {

            if (isConnected) return;
            cube.Select();
            isConnected = true;
        }
    }
}