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
    /// �}���`�I����ʂ���^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        SceneManager.LoadScene("Title Ueno");
    }

    
}
