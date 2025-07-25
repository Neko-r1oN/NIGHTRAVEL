using UnityEngine;

public class SizeChecker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        Debug.Log("横のサイズ：" + sr.bounds.size.x);
        Debug.Log("縦のサイズ：" + sr.bounds.size.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
