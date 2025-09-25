using UnityEngine;
using System.Collections;
using UnityEngine.UI; // UI�R���|�[�l���g�̎g�p
using UnityEngine.InputSystem;

public class menu : MonoBehaviour
{
    Button button;

    private Gamepad gamepad;
    private bool isConnected;
    private bool isClick;

    void Start()
    {
       
        isClick = false;

        // �{�^���R���|�[�l���g�̎擾
        button = GameObject.Find("/Camera/Canvas/Buttons/Button(Solo)").GetComponent<Button>();

        // �ŏ��ɑI����Ԃɂ������{�^���̐ݒ�
        //cube.Select();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isClick = true;
        }

        gamepad = Gamepad.current;

        if (gamepad == null)
        {
            isConnected = false;
        }
        else if (gamepad != null)
        {

            // ���X�e�B�b�N�̓��͒l���擾
            Vector2 stickInput = gamepad.leftStick.ReadValue();

            if (stickInput.x != 0 || stickInput.y != 0)
            {
                if (isClick)
                {
                    button.Select();
                    isClick = false;
                }
            }

            if (isConnected) return;

            button.Select();
            isConnected = true;
        }
    }
}