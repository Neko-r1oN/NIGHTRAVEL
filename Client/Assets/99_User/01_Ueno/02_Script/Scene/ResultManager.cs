using UnityEngine;
using UnityEngine.SceneManagement;

public class RisultManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject obj = GameObject.Find("UISet");

        Destroy(obj);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return))
        {
            SceneManager.LoadScene("Title Ueno");
        }
    }
}
