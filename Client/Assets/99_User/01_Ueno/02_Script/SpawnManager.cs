using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    GameManager gameManager;
    GameObject player;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = GameObject.Find("DrawCharacter");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {// ���ɂ�����
            // �����������@
            gameManager.Enemy.GetComponent<SpriteRenderer>().enabled = true;
            // �v���C���[���X�g�Ƀv���C���[�̏����i�[
            gameManager.Enemy.GetComponent<EnemyController>().Players.Add(player);
        }
    }
}
