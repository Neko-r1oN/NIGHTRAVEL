using UnityEngine;
using UnityEngine.SceneManagement;

public class DataListManager : MonoBehaviour
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
    /// �f�[�^���X�g����^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
