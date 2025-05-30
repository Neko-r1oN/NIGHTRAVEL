using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Animator animator;


    void Start()
    {
        animator=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetBool("IsTouch", true);
            Debug.Log("�v���C���[���R�d�t�B�[���h�ɓ�������");
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("�G���R�d�t�B�[���h�ɓ�������");
        }
    }
}
