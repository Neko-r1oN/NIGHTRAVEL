using UnityEngine;

public class SealedDoor : MonoBehaviour
{
    private GameObject door;
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
            Debug.Log("ÉhÉAÇ…êGÇÍÇÈ");
        }
    }
}
