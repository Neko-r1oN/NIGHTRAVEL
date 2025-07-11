//===================
// �[���X�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
using System.Collections.Generic;
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

    // �X�s�[�h�p�S�[���|�C���g�I�u�W�F�N�g�̃��X�g
    [SerializeField] List<GameObject> pointList;

    List<GameObject> terminalSpawnList;

    GameManager gameManager;

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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        // E�L�[���͂��v���C���[���[���ɐG��Ă���ꍇ�����̒[�������g�p�ł���ꍇ�A�[�����N��
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // �[�����N��
        }


        if (SpawnManager.Instance.TerminalSpawnList.Count <= 0)
        {

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
        System.Random rand = new System.Random();
        int rndNum;
        // �[���^�C�v�ŏ����𕪂���
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // �G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                rndNum = rand.Next(6, 11); // �������𗐐�(6-10)�Őݒ�
                SpawnManager.Instance.TerminalGenerateEnemy(rndNum);   // �G����
                break;

            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                foreach (var point in pointList)
                {   // �e�S�[���|�C���g��\��
                    point.SetActive(true);
                }
                Invoke("TimeUp",10f);   // 10�b��^�C���A�b�v�Ƃ���
                break;

            case (int)TerminalCode.Type_Deal:
                // ����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                rndNum = rand.Next(0, 6); // �������𗐐�(0-5)�Őݒ�

                break;

            case (int)TerminalCode.Type_Jumble:
                // ������܂��̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;

            case (int)TerminalCode.Type_Elite:
                // �G���[�g�G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;

            case (int)TerminalCode.Type_Recycle:
                // ���T�C�N���̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;

            case (int)TerminalCode.Type_Return:
                // �ċA�̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
        }
    }

    /// <summary>
    /// ��V�r�o����
    /// </summary>
    public void GiveReward()
    {
        // �[���^�C�v�ŏ����𕪂���
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // �G�����̏ꍇ
                
                break;
            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ
                Debug.Log("OMFG Reward Here!!!!!");
                break;
            case (int)TerminalCode.Type_Deal:
                // ����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
            case (int)TerminalCode.Type_Jumble:
                // ������܂��̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
            case (int)TerminalCode.Type_Elite:
                // �G���[�g�G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
            case (int)TerminalCode.Type_Recycle:
                // ���T�C�N���̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
            case (int)TerminalCode.Type_Return:
                // �ċA�̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                break;
        }
    }

    /// <summary>
    /// �S�[���|�C���g�ɐG�ꂽ�ۂ̏���
    /// </summary>
    /// <param name="obj"></param>
    public void HitGoalPoint(GameObject obj)
    {
        if (pointList.Contains(obj))    // �n���ꂽ�I�u�W�F�N�g�����X�g���ɂ������ꍇ
        {
            pointList.Remove(obj);  // �������������
            Destroy(obj);   // �����j�󂷂�

            if(pointList.Count <= 0)
            { // ���X�g����ɂȂ����ꍇ�A��V��t�^����
                GiveReward();
            }
        }
    }

    /// <summary>
    /// ���Ԑ؂ꏈ��
    /// </summary>
    private void TimeUp()
    {
        foreach (var point in pointList)
        {   // �e�S�[���|�C���g��\��
        
            point.SetActive(false);
        }
    }
}
