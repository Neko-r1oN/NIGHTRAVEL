using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineManager : MonoBehaviour
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
    /// マルチ選択画面からタイトルに移動
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.Fade("TitleScene", Color.black, 1.0f);   // フェード時間1秒
    }

    
}
