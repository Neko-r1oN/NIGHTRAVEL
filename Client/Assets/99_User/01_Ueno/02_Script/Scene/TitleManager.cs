using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject obj = GameObject.Find("UISet");

        Destroy(obj);
    }

    public void ButtonPush()
    {
        SceneManager.LoadScene("Stage");
    }
}
