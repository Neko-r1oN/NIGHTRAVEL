using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
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
    /// 設定画面からタイトルに移動
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // フェード時間1秒
    }
}
