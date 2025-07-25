using UnityEngine;

public class Abyss : MonoBehaviour
{
    PlayerBase player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

        }
    }
}
