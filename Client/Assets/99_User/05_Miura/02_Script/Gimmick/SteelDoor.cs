//=================
//鋼鉄製自動ドアのスクリプト
//Aouther:y-miura
//Date:20025/06/23
//=================

using DG.Tweening;
using System.Configuration;
using UnityEngine;

public class SteelDoor : GimmickBase
{
    [SerializeField] GameObject doorObj;

    [SerializeField] AudioClip openDoorSE;
    [SerializeField] AudioClip closeDoorSE;
    AudioSource audioSource;

    Vector2 initPos = Vector2.zero;//初期位置
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //初期位置を設定
        Debug.Log(transform.name);
        initPos = doorObj.transform.position;
    }

    /// <summary>
    /// 鋼鉄製自動ドアを開く処理
    /// </summary>
    /// <param name="collision">触れているオブジェクト</param>
    private void OnTriggerStay2D(Collider2D collision)
    {//センサー部分にものが触れている間
        if (collision.CompareTag("Player") && isPowerd == true)
        {//「Player」タグが付いたものが触れている間
            //ドアを開く
            doorObj.transform.DOMoveY(this.transform.position.y+5, 0.5f);
        }
    }

    /// <summary>
    /// 自動ドアを開いたときのSE再生処理
    /// 開く処理の中だと遅延が発生するので別の関数
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {//センサー部分にものが触れたら
        if (collision.CompareTag("Player") && isPowerd == true && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//「Player」タグが付いたものが触れたら
            //ドアを開くSEを再生する
            audioSource.PlayOneShot(openDoorSE);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isPowerd == true && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//「Player」タグが付いたものが離れたら
         //ドアを閉じる
            doorObj.transform.DOMoveY(initPos.y, 0.5f);

            //ドアが閉じるSEを再生する
            audioSource.PlayOneShot(closeDoorSE);
        }
    }

    public override void TurnOnPower()
    {
        if (isPowerd) return;   // すでに起動してある場合は処理しない
        isPowerd = true;
    }

}