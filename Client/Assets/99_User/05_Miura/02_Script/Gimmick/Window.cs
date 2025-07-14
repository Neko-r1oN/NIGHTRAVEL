//====================
// 送風ファンのスクリプト
// Aouther:y-miura
// Date:2025/07/08
//====================

using System.Collections;
using UnityEngine;

public class Window : GimmickBase
{
    AreaEffector2D areaEffector2D;

    public enum Power_ID
    {
        ON = 0,
        OFF
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void TurnOnPower(int triggerID)
    {
        switch (triggerID)
        {
            case 0:
                areaEffector2D = new AreaEffector2D();
                areaEffector2D.gameObject.SetActive(true);
                break;

            case 1:
                areaEffector2D = new AreaEffector2D();
                areaEffector2D.gameObject.SetActive(false);
                break;

            default: 
                break;
        }
    }

    public override void TruggerRequest()
    {

    }
}
