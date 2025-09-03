using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private Transform playerTfm;
    // Update is called once per frame
    void Update()
    {
        // ƒXƒP[ƒ‹‚ð1‚ÉŒÅ’è
        if(playerTfm.localScale.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
