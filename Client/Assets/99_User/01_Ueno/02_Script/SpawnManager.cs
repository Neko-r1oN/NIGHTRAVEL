using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private float distance;
    LayerMask mask;

    private void Start()
    {
        //this.GetComponent<SpriteRenderer>().enabled = false;
        distance = 5.0f;
        mask = LayerMask.GetMask("Default");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.tag == "ground")
        {// 床についたら
            // 透明化解除
            this.GetComponent<SpriteRenderer>().enabled = true;
            
            // プレイヤーリストにプレイヤーの情報を格納
            this.GetComponent<EnemyBase>().enabled = true;
        }*/
    }

    private void Update()
    {
        
    }

    public bool IsGroundCheck()
    {
        Vector3 rayOrigin = this.transform.position;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, mask);

        Debug.DrawRay((Vector2)rayOrigin, Vector2.down * hit.distance, Color.red);
        return hit && hit.collider.gameObject.CompareTag("ground");
    }
}
