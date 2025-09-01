//===================
// �[���X�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
public class Terminal : MonoBehaviour
{
    // �v���C���[���[���ɐG��Ă��邩�̔���ϐ�
    private bool isPlayerIn = false;
    // �g�p����
    private bool isUsed = false;
    // �[���̎��
    public int terminalType;

    [SerializeField] int maxSpawnEnemy;

    public int TerminalType { get { return terminalType; } }

    // �X�s�[�h�p�S�[���|�C���g�I�u�W�F�N�g�̃��X�g
    [SerializeField] List<GameObject> pointList;

    List<GameObject> terminalSpawnList = new List<GameObject>();
    public List<GameObject> TerminalSpawnList { get {  return terminalSpawnList; } set { terminalSpawnList = value; } }
    //List<GameObject> terminalEnemyList = new List<GameObject>();

    GameManager gameManager;
    SpawnManager spawnManager;

    private static Terminal instance;

    public static Terminal Instance
    {
        get { return instance; }
    }

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

    public TerminalCode code;

    public Dictionary<TerminalCode, string> Terminalexplanation = new Dictionary<TerminalCode, string>
    {
        {TerminalCode.None,""},
        {TerminalCode.Type_Enemy,"�o�������G��S�ē|��" },
        {TerminalCode.Type_Speed,"�o�������Q�[�g��S�Ēʂ�" },
        {TerminalCode.Type_Deal,"" },
        {TerminalCode.Type_Recycle,""},
        {TerminalCode.Type_Jumble,"" },
        {TerminalCode.Type_Return,"" },
        {TerminalCode.Type_Elite,"" }
    };

    bool isTerminal;

    public bool IsTerminal {  get { return isTerminal; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        //else
        //{
        //    // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
        //    Destroy(gameObject);
        //}
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isTerminal = false;
        spawnManager = SpawnManager.Instance;
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
        System.Random rand = new System.Random();
        int rndNum;

        UIManager.Instance.DisplayTerminalExplanation();

        // �[���^�C�v�ŏ����𕪂���
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // �G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                isTerminal = true;

                rndNum = rand.Next(1, maxSpawnEnemy); // �������𗐐�(6-10)�Őݒ�

                int childrenCnt = this.gameObject.transform.childCount;

                List<Transform> children = new List<Transform>();

                for ( int i = 0;i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // �G����
                isTerminal = true;
                break;

            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ
                isTerminal = true;
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
                isUsed = true;
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

    private void TerminalGenerateEnemy(int num, Vector2 minPos, Vector2 maxPos)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
        {
            // ��������G�̒��I
            var emitResult = spawnManager.EmitEnemy(spawnManager.EmitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("��������G�̒��I���ʂ�null�̂��߁A�ȍ~�̏������X�L�b�v���܂��B");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;

            EnemyBase enemyBase = spawnManager.IdEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            // �����_���Ȉʒu�𐶐�
            var spawnPostions = spawnManager.CreateEnemyTerminalSpawnPosition(minPos, maxPos);

            Vector3? spawnPos = spawnManager.GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                Vector3 scale = Vector3.one;    // ��U���̂܂�
                var spawnData = spawnManager.CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                spawnManager.SpawnTerminalEnemyRequest(this,spawnData);
            }

            enemyCnt++;
        }   
    }


}
