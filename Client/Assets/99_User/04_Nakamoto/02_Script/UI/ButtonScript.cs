//--------------------------------------------------------------
// ボタンスクリプト [ ButtonScipt.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    //----------------------------------------------------------
    // フィールド

    /// <summary>
    /// ボタンの種類
    /// </summary>
    private enum ButtonType
    {
        Start,
        Option,
        Exit
    }

    [SerializeField] private UnityEngine.UI.Button startButton;

    /// <summary>
    /// 種類保管用
    /// </summary>
    [SerializeField] private ButtonType buttonType;

    //----------------------------------------------------------
    // メソッド

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonPressed);
    }

    void OnStartButtonPressed()
    {
        // ボタンが押されたときの処理
        Debug.Log("スタートボタンが押されました");
    }
}