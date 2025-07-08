using Unity.VisualScripting;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [SerializeField] GameObject deadZone;
    Vector2 respawnPoint;

    private void Start()
    {
        deadZone = GameObject.Find("DeadZone").GetComponent<GameObject>();
        respawnPoint = GameObject.Find("RespawnPoint").transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag =="Player")
        {
            collision.transform.position = respawnPoint;
        }
    }
}
