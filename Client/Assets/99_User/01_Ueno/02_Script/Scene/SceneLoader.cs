using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);    // ���݂̃V�[����UIManager�V�[����ǉ�����
    }
}
