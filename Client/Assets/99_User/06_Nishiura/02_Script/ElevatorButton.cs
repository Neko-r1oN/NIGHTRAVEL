//===================
// �G���x�[�^�[���~�{�^���X�N���v�g
// Author:Nishiura
// Date:2025/07/04
//===================
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // �ΏۃG���x�[�^�̃v���n�u
    [SerializeField] GameObject targetElevator;
    Elevator elevatorScript;
    
    // �{�^���̎��(f:���~t:�㏸)
    public bool buttonType;

    bool isEntered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEntered == true)
        {
            elevatorScript.MoveButton(buttonType);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓��ɓ������ꍇ
        if (collision.transform.tag == "Player")
        {
            isEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓��ɓ������ꍇ
        if (collision.transform.tag == "Player")
        {
            isEntered = false;
        }
    }
}
