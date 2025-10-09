using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConducter : MonoBehaviour
{
    public void Loading()
    {
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);    // ���݂̃V�[����LoadingScene�V�[����ǉ�����
    }    
    
    public void Loaded()
    {
        SceneManager.UnloadSceneAsync("LoadingScene"); //LoadingScene�V�[�����폜
    }    
    
    public void TakeYourPlayer()
    {
        SceneManager.LoadScene("TakeYourPlayerScene", LoadSceneMode.Additive);    // ���݂̃V�[����TakeYourPlayerScene�V�[����ǉ�����
    }
    
    public void SameStartPlayers()
    {
        SceneManager.UnloadSceneAsync("TakeYourPlayerScene"); //TakeYourPlayerScene�V�[�����폜
    }
}
