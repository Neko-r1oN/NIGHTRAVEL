using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);    // ���݂̃V�[����UIManager�V�[����ǉ�����
    }

    /// <summary>
    /// �w�肵�����O�̃V�[�����ēǂݍ��݂��܂��B
    /// </summary>
    /// <param name="sceneName">�ēǂݍ��݂������V�[���̖��O</param>
    public void ReloadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
