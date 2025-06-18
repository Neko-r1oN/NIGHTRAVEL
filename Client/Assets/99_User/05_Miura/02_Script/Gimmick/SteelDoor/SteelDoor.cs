using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SteelDoor : MonoBehaviour
{
    PlayerBase playerBase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Up()
    {
        //ドアが上に移動する処理
        this.transform.DOMoveY(5f, 2f); //2秒かけて上に移動する
    }

    public void Down()
    {
        //ドアの位置をもとに戻す処理
        this.transform.DOKill();
    }
}
