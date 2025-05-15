using UnityEngine;

public class Trap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    TestCharacter testCharacter = new TestCharacter();
    float damage;

    void Start()
    {
        testCharacter.HP = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        damage = testCharacter.HP * 0.3f;
        testCharacter.HP -= damage;

    }
}
