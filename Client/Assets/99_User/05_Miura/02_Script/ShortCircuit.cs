using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //[SerializeField] GameObject ElectronicEffect;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("プレイヤーが漏電フィールドに当たった");
            
            //Instantiate(ElectronicEffect,this.transform.position,this.transform.rotation);
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("敵が漏電フィールドに当たった");
        }
    }
}
