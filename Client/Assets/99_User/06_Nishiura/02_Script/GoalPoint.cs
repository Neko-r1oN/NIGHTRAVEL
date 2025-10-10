//===================
// ゴールポイントスクリプト
// Author:Nishiura
// Date:2025/07/09
//===================
using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    [SerializeField] GameObject terminal;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip getSE;

    Speed speedScript;

    private void Start()
    {
        speedScript = terminal.GetComponent<Speed>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            audioSource.PlayOneShot(getSE);
            speedScript.HitGoalPoint(this.gameObject);
        }
    }
}
