using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayManager : MonoBehaviour
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
    /// �\���v���C����^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    
}
