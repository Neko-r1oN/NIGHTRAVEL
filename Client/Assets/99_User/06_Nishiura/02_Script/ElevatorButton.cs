//===================
// エレベーター昇降ボタンスクリプト
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ElevatorButton : GimmickBase
{
    // 対象エレベータのプレハブ
    [SerializeField] GameObject targetElevator;
    //ボタンオブジェクト
    [SerializeField] GameObject buttonObj;
    Elevator elevatorScript;

    [SerializeField] AudioClip elevatorButtonSE;
    
    AudioSource audioSource;

    // ボタンの種別(f:下降t:上昇)
    public bool buttonType;
    bool isEnterd = false;

    bool isCoolDown = false;

    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEnterd && !elevatorScript.isMoving && !isCoolDown)
        {
            isCoolDown = true;
            Invoke("InvoeCoolTime", 2f);
            TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
        }
    }

    /// <summary>
    /// 連続して押されないようにクールタイムを設ける
    /// </summary>
    void InvoeCoolTime()
    {
        isCoolDown = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内に入った場合
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isEnterd = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // プレイヤーがボタン範囲内から出た場合
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
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

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower()
    {
        if (!isCoolDown)
        {
            isCoolDown = true;
            Invoke("InvoeCoolTime", 2f);
        }

        // 使用音を再生
        audioSource.PlayOneShot(elevatorButtonSE);

        elevatorScript.MoveButton(buttonType);
        MoveButton();
    }
}
