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
}
