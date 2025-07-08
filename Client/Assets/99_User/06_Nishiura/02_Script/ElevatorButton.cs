//===================
// エレベーター昇降ボタンスクリプト
// Author:Nishiura
// Date:2025/07/04
//===================
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // 対象エレベータのプレハブ
    [SerializeField] GameObject targetElevator;
    Elevator elevatorScript;
    
    // ボタンの種別(f:下降t:上昇)
    public bool buttonType;

    bool isEntered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEntered == true)
        {
            elevatorScript.MoveButton(buttonType);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内に入った場合
        if (collision.transform.tag == "Player")
        {
            isEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内に入った場合
        if (collision.transform.tag == "Player")
        {
            isEntered = false;
        }
    }
}
