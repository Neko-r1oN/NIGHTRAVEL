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
        //�h�A�ɐG�ꂽ��&&Y�L�[(��)����������
        if (isDoor == false && Input.GetKeyDown(KeyCode.X))
        {
            //�h�A�j��
            this.gameObject.SetActive(false);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("�h�A�ɐG���");
            isDoor = false;
        }
    }
}
