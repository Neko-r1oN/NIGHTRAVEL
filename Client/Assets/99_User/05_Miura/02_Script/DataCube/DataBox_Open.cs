using UnityEngine;

public class DataBox_Open : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("Destroy", 2f, 0.1f);
    }

    void Destroy()
    {
        if(spriteRenderer.color.a <= 0)
        {
            Destroy(gameObject);
        }

        spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a - 0.4f);
    }
}
