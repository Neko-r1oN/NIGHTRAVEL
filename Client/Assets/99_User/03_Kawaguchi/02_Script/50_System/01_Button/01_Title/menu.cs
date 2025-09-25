using UnityEngine;
using System.Collections;
using UnityEngine.UI; // UIコンポーネントの使用
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

        // ボタンコンポーネントの取得
        button = GameObject.Find("/Camera/Canvas/Buttons/Button(Solo)").GetComponent<Button>();

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