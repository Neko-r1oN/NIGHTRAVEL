using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    float gameTimer = 20;

    GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager")
            .GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // ���쎞�ԍX�V����
        gameTimer -= Time.deltaTime;

        if(gameTimer <= 0 && gameManager.BossFlag == false)
        {
            gameTimer = 0;
            Debug.Log("�{�X�o��");
        }
    }
}
