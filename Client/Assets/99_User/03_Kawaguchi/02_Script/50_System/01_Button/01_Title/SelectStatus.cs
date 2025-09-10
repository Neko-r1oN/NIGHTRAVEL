using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.InputSystem;

public class SelectStatus : MonoBehaviour
{
    [SerializeField] Button cube;

    private Gamepad gamepad;
    private bool isConnected;
    private bool isClick;

    void Start()
    {
       
        isClick = false;

        // 最初に選択状態にしたいボタンの設定
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

            // 左スティックの入力値を取得
            Vector2 stickInput = gamepad.leftStick.ReadValue();

            if (stickInput.x != 0 || stickInput.y != 0)
            {
                if (isClick)
                {
                    cube.Select();
                    isClick = false;
                }
            }

            if (isConnected) return;
            cube.Select();
            isConnected = true;
        }
    }
}
