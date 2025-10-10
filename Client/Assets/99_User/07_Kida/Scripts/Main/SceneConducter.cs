using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConducter : MonoBehaviour
{
    public void Loading()
    {
        Debug.Log("���[�h��");
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);    // ���݂̃V�[����LoadingScene�V�[����ǉ�����
    }    
    
    public void Loaded()
    {
        Debug.Log("���[�h����");
        SceneManager.UnloadSceneAsync("LoadingScene"); //LoadingScene�V�[�����폜
    }    
    
    public void TakeYourPlayer()
    {
        Debug.Log("�v���C���[�҂�");
        SceneManager.LoadScene("TakeYourPlayerScene", LoadSceneMode.Additive);    // ���݂̃V�[����TakeYourPlayerScene�V�[����ǉ�����
    }
    
    public void SameStartPlayers()
    {
        Debug.Log("�v���C���[����");
        SceneManager.UnloadSceneAsync("TakeYourPlayerScene"); //TakeYourPlayerScene�V�[�����폜
        if (SceneManager.GetSceneByName("TakeYourPlayerScene").isLoaded)
            SceneManager.UnloadSceneAsync("TakeYourPlayerScene");
    }
}
