using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int crushNum;
    bool bossFlag = false;

    public bool BossFlag {  get{ return bossFlag; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return))
        {
            crushNum++;
            Debug.Log(crushNum);
            if(crushNum >= 15)
            {
                bossFlag = true;
                Debug.Log("true�ɂȂ�����");
                Debug.Log("�{�X�o��");
            }

            if (bossFlag)
            {
                SceneManager.LoadScene("Result Ueno");
            }
        }
    }
}
