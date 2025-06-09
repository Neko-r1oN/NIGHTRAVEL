using UnityEngine;

//======================================
//奈落に落ちた時に、少し前の位置に復活するための
//チェックポイントのスクリプト
//作成者:三浦有稀
//更新:2025/06/05
//=======================================

public class CheckPoint : MonoBehaviour
{
    public bool isCheck=false;
    GameObject spawnObject=new GameObject();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnObject = this.gameObject.transform.GetChild(0).GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isCheck = true;
        }
    }
}
