using UnityEngine;

public class SizeChecker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        Debug.Log("���̃T�C�Y�F" + sr.bounds.size.x);
        Debug.Log("�c�̃T�C�Y�F" + sr.bounds.size.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
