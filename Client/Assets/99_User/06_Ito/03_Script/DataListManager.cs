using UnityEngine;
using UnityEngine.SceneManagement;

public class DataListManager : MonoBehaviour
{
    [SerializeField] GameObject playerInfo;
    [SerializeField] GameObject enemyInfo; 
    [SerializeField] GameObject relicInfo;

    private void Start()
    {
        playerInfo.SetActive(true);
        enemyInfo.SetActive(false);
        relicInfo.SetActive(false);

    }
    public void PushPlayer()
    {
        playerInfo.SetActive(true);
        enemyInfo.SetActive(false);
        relicInfo.SetActive(false);
    }

    public void PushEnemy()
    {
        playerInfo.SetActive(false);
        enemyInfo.SetActive(true);
        relicInfo.SetActive(false);
    }

    public void PushRelic()
    {
        playerInfo.SetActive(false);
        enemyInfo.SetActive(false);
        relicInfo.SetActive(true);
    }

    /// <summary>
    /// データリストからタイトルに移動
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.Fade("TitleScene", Color.black, 1.0f);   // フェード時間1秒
    }
}
