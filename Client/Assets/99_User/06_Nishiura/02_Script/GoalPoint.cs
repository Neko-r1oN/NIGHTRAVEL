//===================
// ゴールポイントスクリプト
// Author:Nishiura
// Date:2025/07/09
//===================
using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    [SerializeField] GameObject terminal;

    Terminal terminalScript;

    private void Start()
    {
        terminalScript = terminal.GetComponent<Terminal>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            terminalScript.HitGoalPoint(this.gameObject);
        }
    }
}
