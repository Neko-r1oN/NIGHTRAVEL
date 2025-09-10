//===================
// エレベーター昇降ボタンスクリプト
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // 対象エレベータのプレハブ
    [SerializeField] GameObject targetElevator;
    //ボタンオブジェクト
    [SerializeField] GameObject buttonObj;
    Elevator elevatorScript;
    
    // ボタンの種別(f:下降t:上昇)
    public bool buttonType;

    bool isEnterd = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEnterd && !elevatorScript.isMoving)
        {
            elevatorScript.MoveButton(buttonType);
            MoveButton();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内に入った場合
        if (collision.transform.tag == "Player")
        {
            isEnterd = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内から出た場合
        if (collision.transform.tag == "Player")
        {
            isEnterd = false;
        }
    }

    void MoveButton() 
    {
        buttonObj.transform.localPosition =  new Vector2(0f,-0.08f);
        //Sequenceのインスタンスを作成
        var sequence = DOTween.Sequence();

        //Appendで動作を追加していく
        sequence.Append(buttonObj.transform.DOMoveY(buttonObj.transform.position.y - 0.3f, 0.5f))
                 .AppendInterval(0.1f)
                 .Append(buttonObj.transform.DOMoveY(buttonObj.transform.position.y, 0.5f));
        //Playで実行
        sequence.Play();
    }
}
