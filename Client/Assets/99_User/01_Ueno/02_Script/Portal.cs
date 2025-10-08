using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    enum ROLE_TYPE
    {
        Result,
        NextStage
    }

    [SerializeField] ROLE_TYPE roleType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        // プレイヤーが端末付近に接近した場合
        if (collision.transform.tag == "Player")
        {
            if (roleType == ROLE_TYPE.Result)
            {
                if(Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Interact"))
                    UIManager.Instance.DisplayEndGameWindow();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Interact"))
                    UIManager.Instance.DisplayNextStageWindow();
            }
        }
    }
}
