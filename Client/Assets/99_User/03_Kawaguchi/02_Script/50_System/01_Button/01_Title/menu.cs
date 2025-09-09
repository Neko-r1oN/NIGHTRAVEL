using UnityEngine;
using System.Collections;
using UnityEngine.UI; // UIコンポーネントの使用
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

        // ボタンコンポーネントの取得
        cube = GameObject.Find("/Canvas/Buttons/Button(Solo)").GetComponent<Button>();

        // 最初に選択状態にしたいボタンの設定
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