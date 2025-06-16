using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SealedDoor : MonoBehaviour
{
    [SerializeField] GameObject DoorFragment;
    GameObject player;
    List<Transform> fragmentList=new List<Transform>();
    Rigidbody2D rigidbody2D;
    public bool isDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDoor = false;
        rigidbody2D=this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //ドアに触れていて、攻撃したら
        if (isDoor == true && Input.GetKeyDown(KeyCode.X))
        {
            GameObject fragment;
            fragment= Instantiate(DoorFragment, new Vector2(this.transform.position.x, this.transform.position.y - 2), this.transform.rotation);

            for (int i = 0; i < fragment.transform.childCount; i++)
            {
                if(this.transform.position.x- player.transform.position.x>=0)
                {
                    fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200));
                }
                else
                {
                    fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200));
                }
            }

            //ドアを壊す
            Destroy(this.gameObject);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player=collision.gameObject;
            isDoor = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Attack"))
        {
            player = collision.gameObject;
            GameObject fragment;
            fragment = Instantiate(DoorFragment, new Vector2(this.transform.position.x, this.transform.position.y - 2), this.transform.rotation);

            for (int i = 0; i < fragment.transform.childCount; i++)
            {
                if (this.transform.position.x - player.transform.position.x >= 0)
                {
                    fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200));
                }
                else
                {
                    fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200));
                }
            }

            //ドアを壊す
            Destroy(this.gameObject);

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
