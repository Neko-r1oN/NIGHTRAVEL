using System.ComponentModel;
using UnityEngine;

public class SealedDoor : MonoBehaviour
{
    [SerializeField] GameObject ExplosionEffect;
    public bool isDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDoor = false;
    }

    // Update is called once per frame
    void Update()
    {
        //ドアに触れていて、攻撃したら
        if (isDoor == true && Input.GetKeyDown(KeyCode.X))
        {
            //ドアを壊す
            Destroy(this.gameObject);
            Instantiate(ExplosionEffect, this.transform.position, this.transform.rotation);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isDoor = true;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isDoor = false;
        }
    }
}
