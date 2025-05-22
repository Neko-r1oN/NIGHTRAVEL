using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return))
        {
            SceneManager.LoadScene("SampleGameUeno");
        }*/
    }

    public void ButtonPush()
    {
        SceneManager.LoadScene("SampleGameUeno");
    }
}
