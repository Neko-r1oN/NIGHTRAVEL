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
        //�h�A����Ɉړ����鏈��
        this.transform.DOMoveY(5f, 2f); //2�b�����ď�Ɉړ�����
    }

    public void Down()
    {
        //�h�A�̈ʒu�����Ƃɖ߂�����
        this.transform.DOKill();
    }
}
