using UnityEngine;

public class Trap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public TestCharacter testCharacter = new TestCharacter();
    public float damage;

    void Start()
    {
        testCharacter.HP = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void Damage()
    //{
    //    damage = testCharacter.HP * 0.3f;
    //    testCharacter.HP -= damage;
    //}
}
