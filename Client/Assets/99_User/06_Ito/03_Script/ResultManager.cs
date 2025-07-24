using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// リザルト画面からタイトルに移動
    /// </summary>
    public void BackTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
