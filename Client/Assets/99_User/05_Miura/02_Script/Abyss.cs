using UnityEngine;

public class Abyss : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            Debug.Log("�v���C���[���ޗ��ɗ�����");
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("�G���ޗ��ɗ�����");
        }
    }
}
