//===================
// �[���X�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
using Unity.VisualScripting;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    // �v���C���[���[���ɐG��Ă��邩�̔���ϐ�
    private bool isPlayerIn = false;
    // �g�p����
    private bool isUsed = false;
    // �[���̎��
    public int terminalType;

    // �[���^�C�v�񋓌^
    public enum TerminalCode 
    {
        None = 0,
        Type_Enemy,
        Type_Speed,
        Type_Deal,
        Type_Recycle,
        Type_Jumble,
        Type_Return,
        Type_Elite
    }

    private void Start()
    {

    }

    private void Update()
    {
        // E�L�[���͂��v���C���[���[���ɐG��Ă���ꍇ�����̒[�������g�p�ł���ꍇ�A�[�����N��
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // �[�����N��
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �v���C���[���[���t�߂ɐڋ߂����ꍇ
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = true;  // �G�ꂽ���ƂƂ���
            Debug.Log("You Touched Terminal");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // �v���C���[���[�����痣�ꂽ�ꍇ
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = false; // �G��Ă��Ȃ����ƂƂ���
            Debug.Log("No Terminal");
        }
    }

    /// <summary>
    /// �[���N������
    /// </summary>
    private void BootTerminal()
    {
        // �[���^�C�v�ŏ����𕪂���
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // �G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                SpawnManager.Instance.GenerateEnemy(1);   // �G����
                break;
            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ

                break;
            case (int)TerminalCode.Type_Deal:
                // ����̏ꍇ

                break;
            case (int)TerminalCode.Type_Jumble:
                // ������܂��̏ꍇ

                break;
            case (int)TerminalCode.Type_Elite:
                // �G���[�g�G�����̏ꍇ

                break;
            case (int)TerminalCode.Type_Recycle:
                // ���T�C�N���̏ꍇ

                break;
            case (int)TerminalCode.Type_Return:
                // �ċA�̏ꍇ

                break;
        }
    }
}
