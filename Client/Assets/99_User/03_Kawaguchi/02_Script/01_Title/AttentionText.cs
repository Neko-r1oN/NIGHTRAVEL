using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;  //DOTweenを使うときはこのusingを入れる

/// <summary>
/// ゲーム起動時表示テキストクラス
/// </summary>
public class AttentionText : MonoBehaviour
{
    [SerializeField] GameObject text;           //テキスト
    [SerializeField] GameObject image;    //プロダクト画像

    [SerializeField] float startTime;     //描画開始時間

    private Text textColor;     //色変更用(テキスト)
    private Image imgColor;     //色変更用(画像)

    private float finishTime = 3.0f;     //表示時間
    private float timer;                 //繰り返す間隔

    public bool isChange;   //表示を終えたら遷移するか

    [Header("スタートカラー")]
    [SerializeField]
    Color32 startColor = new Color32(255, 255, 255, 0);

    [Header("エンドカラー")]
    [SerializeField]
    Color32 endColor = new Color32(255, 255, 255, 255);

    void Start()
    {
        if (text)
        {
            textColor = text.GetComponent<Text>();
            textColor.color = startColor;
        }
        if (image)
        {
            imgColor = image.GetComponent<Image>();
            imgColor.color = startColor;
        }

        timer = 0.0f;
    }

    void Update()
    {
        timer += Time.deltaTime;     //時間をカウントする

        if (timer >= startTime)
        {
            if (text) textColor.color = Color.Lerp(textColor.color, new Color(1, 1, 1, 1), 2.0f * Time.deltaTime);      //テキスト
            if (imgColor) imgColor.color = Color.Lerp(imgColor.color, new Color(1, 1, 1, 1), 2.0f * Time.deltaTime);     //プロダクト画像

            if (timer >= startTime + finishTime)
            {
                if (text) textColor.color = Color.Lerp(textColor.color, new Color(0, 0, 0, -3), 2.0f * Time.deltaTime);
                if (imgColor) imgColor.color = Color.Lerp(imgColor.color, new Color(0, 0, 0, -3), 2.0f * Time.deltaTime);

                if (isChange) Invoke("StartTitleScene", 1.0f);
            }
        }
    }

    public void StartTitleScene()
    {
        // シーン遷移
        //SceneManager.LoadScene("TitleScene");
        Initiate.DoneFading();
        Initiate.Fade("Title Ueno", new Color(0, 0, 0, 0), 2.0f);
    }
}
