//===================
// �[���X�N���v�g
// Author:Nishiura
// Date:2025/07/01
//===================
using DG.Tweening;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Terminal : MonoBehaviour
{
    // �v���C���[���[���ɐG��Ă��邩�̔���ϐ�
    private bool isPlayerIn = false;
    // �g�p����
    private bool isUsed = false;
    // �[���̎��
    public int terminalType;

    //��������G�̍ő吔
    [SerializeField] int maxSpawnEnemy;

    public int TerminalType { get { return terminalType; } }

    protected bool isBoot = false;    // true�FON, false�FOFF
    public bool IsBoot { get { return isBoot; } set { isBoot = value; } }

    //UIManager
    UIManager uiManager;
    public GameObject TerminalObj {  get; private set; }

    //TimerDirecter
    TimerDirector timerDirector;

    //��������
    public int limitTime;

    bool isTerminal;

    //�[���̃A�C�R��
    [SerializeField] GameObject terminalIcon;

    // �X�s�[�h�p�S�[���|�C���g�I�u�W�F�N�g�̃��X�g
    [SerializeField] List<GameObject> pointList;
    [SerializeField] List<Transform> relicSpawnPoints = new List<Transform>();

    List<GameObject> terminalSpawnList = new List<GameObject>();
    public List<GameObject> TerminalSpawnList { get { return terminalSpawnList; } set { terminalSpawnList = value; } }
    List<GameObject> terminalEnemyList = new List<GameObject>();

    GameManager gameManager;
    SpawnManager spawnManager;
    PlayerBase player;

    //�����b�N�Ǘ��N���X
    RelicManager relicManager;

    private static Terminal instance;

    public static Terminal Instance
    {
        get { return instance; }
    }

    // �[���^�C�v�񋓌^
    public enum TerminalCode
    {
        Type_Enemy = 1,
        Type_Speed,
        Type_Deal,
        Type_Jumble,
        Type_Elite,
        Type_Boss
    }

    public TerminalCode code;

    public Dictionary<TerminalCode, string> Terminalexplanation = new Dictionary<TerminalCode, string>
    {
        {TerminalCode.Type_Enemy,"�o�������G��S�ē|��" },
        {TerminalCode.Type_Speed,"�o�������Q�[�g��S�Ēʂ�" },
        {TerminalCode.Type_Deal,"�������" },
        {TerminalCode.Type_Jumble,"" },
        {TerminalCode.Type_Elite,"�o�������G���[�g�G��S�ē|��" },
        {TerminalCode.Type_Boss,"" }
    };

    


    public bool IsTerminal { get { return isTerminal; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = UIManager.Instance;
        spawnManager = SpawnManager.Instance;
        isTerminal = false;
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
            player = collision.gameObject.GetComponent<PlayerBase>();

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
    /// �M�~�b�N�̋N��
    /// </summary>
    public virtual void TurnOnPower()
    {
        isBoot = true;
    }

    /// <summary>
    /// �M�~�b�N�N�����N�G�X�g
    /// </summary>
    /// <param name="player">�N�������v���C���[</param>
    public void TurnOnPowerRequest(GameObject player)
    {
        // �I�t���C���p
        if (!RoomModel.Instance)
        {
            TurnOnPower();
        }
        // �}���`�v���C�� && �N�������l���������g�̏ꍇ
        else if (RoomModel.Instance && player == CharacterManager.Instance.PlayerObjSelf)
        {
            // �T�[�o�[�ɑ΂��ă��N�G�X�g����
        }
    }

    /// <summary>
    /// �[���N������
    /// </summary>
    private void BootTerminal()
    {
        System.Random rand = new System.Random();
        int rndNum; //�G������

        //UIManager.Instance.DisplayTerminalExplanation();

        // �[���^�C�v�ŏ����𕪂���
        switch (terminalType)
        {
            case (int)TerminalCode.Type_Enemy:
                // �G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                isTerminal = true;

                rndNum = rand.Next(1,maxSpawnEnemy); // �������𗐐�(6-10)�Őݒ�

                int childrenCnt = this.gameObject.transform.childCount;

                List<Transform> children = new List<Transform>();

                //�[���̃A�C�R����1.5�b�����ăt�F�[�h�A�E�g����
                //terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                for (int i = 0; i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // �G����


                break;

            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ
                isTerminal = true;
                isUsed = true;  // �g�p�ς݂ɂ���
                foreach (var point in pointList)
                {   // �e�S�[���|�C���g��\��
                    point.SetActive(true);
                }

                //�[���̃A�C�R����1.5�b�����ăt�F�[�h�A�E�g����
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                //�J�E���g�_�E������
                InvokeRepeating("CountDown", 1, 1);

                break;

            case (int)TerminalCode.Type_Deal:
                // ����̏ꍇ

                //�[���̃A�C�R����1.5�b�����ăt�F�[�h�A�E�g����
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                isUsed = true;  // �g�p�ς݂ɂ���
                rndNum = rand.Next(0, 6); // �������𗐐�(0-5)�Őݒ�

                //�_���[�W��^����
                if (terminalType == 3 && isUsed == true)
                {
                    DealDamage();
                }

                break;

            case (int)TerminalCode.Type_Jumble:
                // ������܂��̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                //�[���̃A�C�R����1.5�b�����ăt�F�[�h�A�E�g����
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                JumbleRelic();

                break;

            case (int)TerminalCode.Type_Elite:
                // �G���[�g�G�����̏ꍇ
                // �G�����̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���
                isTerminal = true;

                rndNum = rand.Next(6, maxSpawnEnemy); // �������𗐐�(6-10)�Őݒ�

                childrenCnt = this.gameObject.transform.childCount;

                children = new List<Transform>();

                //�[���̃A�C�R����1.5�b�����ăt�F�[�h�A�E�g����
                terminalIcon.GetComponent<Renderer>().material.DOFade(0, 1.5f);

                for (int i = 0; i < childrenCnt; i++)
                {
                    children.Add(this.gameObject.transform.GetChild(i));
                }

                TerminalGenerateEnemy(rndNum, children[0].position, children[1].position);   // �G����

                isTerminal = true;
                break;

            case (int)TerminalCode.Type_Boss:
                if (SpawnManager.Instance.CrashNum >= SpawnManager.Instance.KnockTermsNum)
                {
                    isUsed = true;

                    SpawnManager.Instance.SpawnBoss();
                }

                break;
        }
    }

    [ContextMenu("GiveRewardRequest")]
    private void GiveRewardRequest()
    {
        Stack<Vector2> posStack = new Stack<Vector2>();

        foreach (var point in relicSpawnPoints)
        {
            posStack.Push(point.position);
        }

        //�����b�N��r�o����
        RelicManager.Instance.DropRelicRequest(posStack, false);
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
                //�ʏ�G�Ƃ̃o�g���̏ꍇ
                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Speed:
                // �X�s�[�h�̏ꍇ

                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                //�J�E���g�_�E�����~����
                CancelInvoke("CountDown");

                //��V��r�o
                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Deal:
                // ����̏ꍇ

                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Jumble:
                // ������܂��̏ꍇ
                isUsed = true;  // �g�p�ς݂ɂ���

                GiveRewardRequest();

                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                break;
            case (int)TerminalCode.Type_Elite:
                // �G���[�g�G�����̏ꍇ
                isUsed = true;

                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
            case (int)TerminalCode.Type_Boss:
                isUsed = true;

                // �^�[�~�i���̌��ʂ��I������
                isTerminal = false;

                UIManager.Instance.DisplayTimeInstructions();

                GiveRewardRequest();

                break;
        }
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// </summary>
    public void CountDown()
    {
        //limitTIme��1�����炷
        limitTime--;

        var span = new TimeSpan(0, 0, (int)limitTime);
        TimerDirector.Instance.Timer.text = span.ToString(@"mm\:ss");

        //�������Ԃ�cowntDownText�ɔ��f����
        //countDownText.text=limitTime.ToString();

        //�������Ԃ�0�ȉ��ɂȂ�����(���Ԑ؂�)
        if (limitTime <= 0)
        {
            //isTerminal��false�ɂ���
            isTerminal = false;

            //limitTime��0�ɂ���
            limitTime = 0;

            //�J�E���g�_�E�����~����
            CancelInvoke("CountDown");


            //�S�[���|�C���g���폜����
            foreach (GameObject obj in pointList)
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// �G��������
    /// </summary>
    /// <param name="num"></param>
    /// <param name="minPos">�����ŏ��ʒu</param>
    /// <param name="maxPos">�����ő�ʒu</param>
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
                if (code == TerminalCode.Type_Elite)
                {
                    var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                    Vector3 scale = Vector3.one;    // ��U���̂܂�
                    // var spawnData = spawnManager.CreateTerminalSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                    //spawnManager.SpawnTerminalEnemyRequest(this, spawnData);
                }
                else
                {
                    var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                    Vector3 scale = Vector3.one;    // ��U���̂܂�
                    var spawnData = spawnManager.CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                    //spawnManager.SpawnTerminalEnemyRequest(this, spawnData);
                }
            }

            enemyCnt++;
        }
    }

    /// <summary>
    /// ����[����HP�����炷����
    /// </summary>
    public void DealDamage()
    {
        int HP = player.HP;

        //���炷�ʂ͌��݂�HP��50%
        int damege = Mathf.FloorToInt(HP * 0.5f);

        //dealDamage��0��菬������0��������
        if (damege <= 0)
        {
            //dealDamage��1�ɂ���
            damege = 1;
        }

        //HP�����炷
        player.ApplyDamage(damege);

        //�����b�N��r�o����
        GiveReward();
    }

    public void JumbleRelic()
    {
        GiveReward();
        //RelicManager.Instance.ShuffleRelic();
    }

    
}
