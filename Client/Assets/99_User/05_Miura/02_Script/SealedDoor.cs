using UnityEngine;

public class SealedDoor : MonoBehaviour
{
    public bool isDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDoor = true;
    }

    // Update is called once per frame
    void Update()
    {
        //ドアに触れたら&&Yキー(仮)を押したら
        if (isDoor == false && Input.GetKeyDown(KeyCode.X))
        {
            //ドア破壊
            this.gameObject.SetActive(false);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ドアに触れる");
            isDoor = false;
        }
    }
}
