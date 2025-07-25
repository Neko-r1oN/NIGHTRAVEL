using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
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
    /// İ’è‰æ–Ê‚©‚çƒ^ƒCƒgƒ‹‚ÉˆÚ“®
    /// </summary>
    public void BackTitleButton()
    {
        SceneManager.LoadScene("Title Ueno");
    }
}
