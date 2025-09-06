//----------------------------------------------------
// �G�����N���X
// Author : Souma Ueno
//----------------------------------------------------
using DG.Tweening;
using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region �G��������
    [SerializeField] Vector2 spawnRange;
    [SerializeField] float spawnRangeOffset;
    #endregion

    #region �G�����֘A
    [SerializeField] int maxSpawnCnt; // �}�b�N�X�X�|�[����
    public int MaxSpawnCnt { get { return maxSpawnCnt; } }
    [SerializeField] int knockTermsNum;      // �{�X�̃G�l�~�[�̌��j������
    public int KnockTermsNum { get { return knockTermsNum; } }
    [SerializeField] float spawnProbability = 0.05f; // 5%�̊m�� (0.0����1.0�̊ԂŎw��)
    int fivePercentOfMaxFloor;
    List<Vector3> enemySpawnPosList = new List<Vector3>();
    #endregion

    #region �X�e�[�W���
    [SerializeField] Transform stageMin;             // ���X�|�[���͈�A
    [SerializeField] Transform stageMax;             // ���X�|�[���͈�B
    [SerializeField] Transform minTerminalRespawn;       // �^�[�~�i�����X�|�[���͈�
    [SerializeField] Transform maxTerminalRespawn;       // �^�[�~�i�����X�|�[���͈�

    #region �O���Q��
    public Transform StageMinPoint { get { return stageMin; } }
    public Transform StageMaxPoint { get { return stageMax; } }
    #endregion

    #endregion

    #region �G�֘A
    [SerializeField] List<GameObject> enemyPrefabs;      // �G�l�~�[�̃v���t�@�u���X�g
    [SerializeField] List<EnumManager.ENEMY_TYPE> emitEnemyTypes;   // �����Ώۂ̓G�̎��
    public List<EnumManager.ENEMY_TYPE> EmitEnemyTypes { get { return emitEnemyTypes; } }

    [SerializeField] Dictionary<EnumManager.ENEMY_TYPE, GameObject> idEnemyPrefabPairs;
    public Dictionary<EnumManager.ENEMY_TYPE, GameObject> IdEnemyPrefabPairs { get { return idEnemyPrefabPairs; } }

    int eliteEnemyCnt;
    List<GameObject> terminalEnemyList = new List<GameObject>();
    public List<GameObject> TerminalEnemyList { get { return terminalEnemyList; } }
    #endregion

    #region �{�X�֘A
    GameObject boss;            // �{�X
    public GameObject Boss { get { return boss; }}
    [SerializeField] GameObject bossTerminal;
    [SerializeField] ENEMY_TYPE bossId;
    bool isSpawnBoss;           // �{�X���������ꂽ���ǂ���
    public bool IsSpawnBoss { get {  return isSpawnBoss; } set {  isSpawnBoss = value; } }
    #endregion

    int crashNum = 0; �@�@�@�@�@// ���j��
    public int CrashNum { get { return crashNum; } set { crashNum = value; } }

    CharacterManager characterManager;

    private static SpawnManager instance;

    public static SpawnManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }

        // RoomModel�����s���ĂȂ��ꍇ�͂������牺�͎��s���Ȃ�
        if (RoomModel.Instance == null) return;

        // ���[�����f����OnSpawndEnemy���s���ɁA���̒���OnSpawnEnemy�����s����
        RoomModel.Instance.OnSpawndEnemy += this.OnSpawnEnemy;
    }

    private void Start()
    {
        SetEnemyPrefabList();

        characterManager = CharacterManager.Instance;

        // �G���������5%���擾
        fivePercentOfMaxFloor = (int)((float)maxSpawnCnt * spawnProbability);

        StartCoroutine(WaitAndStartCoroutine(5f));
    }

    private void OnDisable()
    {
        // RoomModel�����݂���Ȃ�A�o�^�ς݂̃A�N�V����������
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSpawndEnemy -= this.OnSpawnEnemy;
    }

    IEnumerator SpawnCoroutin()
    {
        while (true)
        {
            if (GameManager.Instance.IsGameStart)
            {

                if (crashNum >= knockTermsNum && IsSpawnBoss)
                {
                    SpawnBoss();
                }

                if (characterManager.Enemies.Count < maxSpawnCnt && !GameManager.Instance.IsBossDead)
                {// �X�|�[���񐔂����E�ɒB���Ă��邩
                    if (!isSpawnBoss)
                    {
                        foreach (var player in CharacterManager.Instance.PlayerObjs.Values)
                        {
                            if (characterManager.Enemies.Count > maxSpawnCnt) break;
                            if (!player) continue;
                            if (characterManager.Enemies.Count < maxSpawnCnt / 2)
                            {// �G��100�̂��Ȃ��ꍇ
                                GenerateEnemy(Random.Range(7, 11), player.transform.position);
                            }
                            else
                            {// ����ꍇ
                                GenerateEnemy(1, player.transform.position);
                            }

                        }
                    }
                }
            }

            yield return new WaitForSeconds(10);
        }
    }

    IEnumerator WaitAndStartCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SpawnCoroutin());
    }

    /// <summary>
    /// �G�̃v���t�@�u�����܂Ƃ߂�
    /// </summary>
    void SetEnemyPrefabList()
    {
        idEnemyPrefabPairs = new Dictionary<EnumManager.ENEMY_TYPE, GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            Debug.Log(prefab.GetComponent<EnemyBase>().EnemyTypeId + "�F" + prefab.name);
            idEnemyPrefabPairs.Add(prefab.GetComponent<EnemyBase>().EnemyTypeId, prefab);
        }
    }

    /// <summary>
    /// �G�̃X�|�[���\�͈͔��菈��
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    public (Vector3 minRange, Vector3 maxRange) CreateEnemyTerminalSpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < minTerminalRespawn.position.y)
        {
            minRange.y = minTerminalRespawn.position.y;
        }

        if (minPoint.x < minTerminalRespawn.position.x)
        {
            minRange.x = minTerminalRespawn.position.x;
        }

        if (maxPoint.y > maxTerminalRespawn.position.y)
        {
            maxRange.y = maxTerminalRespawn.position.y;
        }

        if (maxPoint.x > maxTerminalRespawn.position.x)
        {
            maxRange.x = maxTerminalRespawn.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// �G�����̈ʒu���菈��
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    public Vector3? GenerateEnemySpawnPosition(Vector3 minRange, Vector3 maxRange, EnemyBase enemyBase)
    {
        // ���s��
        int loopMax = 100;

        for (int i = 0; i < loopMax; i++)
        {
            int seed = System.DateTime.Now.Millisecond + i;
            Random.InitState(seed);  // Unity�̗����ɃV�[�h��ݒ�

            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector2? pos = IsGroundCheck(spawnPos);
            if (pos != null && !enemySpawnPosList.Contains(spawnPos))
            {
                // list�̒��ɂȂ��ꍇ�A���X�g��add
                enemySpawnPosList.Add(spawnPos);

                LayerMask mask = LayerMask.GetMask("Default");

                Vector2 result = (Vector2)pos;

                result.y += enemyBase.SpawnGroundOffset;

                if (!Physics2D.OverlapCircle(new Vector2(result.x, result.y + 1), 0.8f, mask))
                {
                    return result;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// �G��������
    /// </summary>
    public void GenerateEnemy(int num,Vector2 playerPos)
    {
        // �}�X�^�N���C�A���g�ȊO�͏��������Ȃ�
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        List<SpawnEnemyData> spawnEnemyDatas = new List<SpawnEnemyData>();
        for (int i = 0; i < num; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);  // �����̃V�[�h�l���X�V

            if (characterManager.Enemies.Count > maxSpawnCnt)
            {
                return; 
            }

            // ��������G�̒��I
            var emitResult = EmitEnemy(emitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("��������G�̒��I���ʂ�null�̂��߁A�ȍ~�̏������X�L�b�v���܂��B");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;

            EnemyBase enemyBase = idEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            Vector2 spawnRight = playerPos + Vector2.right * spawnRangeOffset;
            Vector2 spawnLeft = playerPos + Vector2.left * spawnRangeOffset;

            Vector2 minSpawnRight = spawnRight - spawnRight / 2;
            Vector2 maxSpawnRight = spawnRight + spawnRight / 2;

            Vector2 minSpawnLeft = spawnLeft - spawnLeft / 2;
            Vector2 maxSpawnLeft = spawnLeft + spawnLeft / 2;

            Vector3? spawnRightPosCandidate = null, spawnLeftPosCandidate = null, spawnPos = null;

            spawnRightPosCandidate = GenerateEnemySpawnPosition(minSpawnRight, maxSpawnRight, enemyBase);
            spawnLeftPosCandidate = GenerateEnemySpawnPosition(minSpawnLeft, maxSpawnLeft, enemyBase);

            if (spawnLeftPosCandidate != null && spawnRightPosCandidate != null)
            {
                int rand = Random.Range(0, 2);

                if(rand == 0)
                {
                    spawnPos = spawnRightPosCandidate;
                }
                else
                {
                    spawnPos = spawnLeftPosCandidate;
                }
            }
            else if (spawnLeftPosCandidate != null || spawnRightPosCandidate != null)
            {
                spawnPos = spawnRightPosCandidate == null ? spawnLeftPosCandidate : spawnRightPosCandidate;
            }

            // �������W���m�肵�Ă���ꍇ�͓G�𐶐�����
            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // ��U���̂܂�
                spawnEnemyDatas.Add(CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType));
            }
        }
        // �����X�|�[�����X�g������
        enemySpawnPosList.Clear();
        SpawnEnemyRequest(spawnEnemyDatas.ToArray());
    }

    /// <summary>
    /// �[�����쎞�̓G��������
    /// </summary>
    public void TerminalGenerateEnemy(int num, Vector2 minPos, Vector2 maxPos)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
        {
            // ��������G�̒��I
            var emitResult = EmitEnemy(emitEnemyTypes.ToArray());
            if (emitResult == null)
            {
                Debug.LogWarning("��������G�̒��I���ʂ�null�̂��߁A�ȍ~�̏������X�L�b�v���܂��B");
                continue;
            }
            ENEMY_TYPE enemyType = (ENEMY_TYPE)emitResult;

            EnemyBase enemyBase = idEnemyPrefabPairs[enemyType].GetComponent<EnemyBase>();

            // �����_���Ȉʒu�𐶐�
            var spawnPostions = CreateEnemyTerminalSpawnPosition(minPos, maxPos);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByTerminal;
                Vector3 scale = Vector3.one;    // ��U���̂܂�
                var spawnData = CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);

                SpawnEnemyRequest(null, spawnData);

                // �[������o���G�����X�g�ɒǉ�
                terminalEnemyList = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByTerminal);

                enemyCnt++;
            }
        }
    }

    /// <summary>
    /// ���`�F�b�N
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <returns></returns>
    private Vector2? IsGroundCheck(Vector3 rayOrigin)
    {
        LayerMask mask = LayerMask.GetMask("Default");

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, mask);

        Debug.DrawRay((Vector2)rayOrigin, Vector2.down * hit.distance, Color.red);
        if (hit && hit.collider.gameObject.CompareTag("ground"))
        {
            return hit.point;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// �G�̏o���m���̒��o
    /// </summary>
    /// <param name="probs"></param>
    /// <returns></returns>
    public int Choose(float[] probs)
    {
        float total = 0;

        //�z��̗v�f�������ďd�݂̌v�Z
        foreach (float elem in probs)
        {
            total += elem;
        }

        //�d�݂̑�����0����1.0�̗����������Ē��I���s��
        float randomPoint = Random.value * total;

        //i���z��̍ő�v�f���ɂȂ�܂ŌJ��Ԃ�
        for (int i = 0; i < probs.Length; i++)
        {
            //�����_���|�C���g���d�݂�菬�����Ȃ�
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                //�����_���|�C���g���d�݂��傫���Ȃ炻�̒l�������Ď��̗v�f��
                randomPoint -= probs[i];
            }
        }

        //�������P�̎��A�z�񐔂�-�P���v�f�̍Ō�̒l��Choose�z��ɖ߂��Ă���
        return probs.Length - 1;
    }

    /// <summary>
    /// ��������G�̒��I����
    /// </summary>
    public ENEMY_TYPE? EmitEnemy(params ENEMY_TYPE[] types)
    {
        // ���v�̏d�݂��擾
        int tatalWeight = 0;
        List<EnemyBase> enemies = new List<EnemyBase>();
        foreach(var type in types)
        {
            if (idEnemyPrefabPairs.ContainsKey(type))
            {
                var enemy = idEnemyPrefabPairs[type].GetComponent<EnemyBase>();
                enemies.Add(enemy);
                tatalWeight += enemy.SpawnWeight;
            }
        }

        // �L�����N�^�[ID����ɏ����Ƀ\�[�g
        enemies.Sort((a, b) => a.EnemyTypeId.CompareTo(b.EnemyTypeId));

        EnumManager.ENEMY_TYPE? entryType = null;
        int emitRnd = Random.Range(1, tatalWeight + 1);
        int currentWeight = 0;
        foreach(EnemyBase enemy in enemies)
        {
            currentWeight += enemy.SpawnWeight;
            if (emitRnd <= currentWeight)
            {
                entryType = enemy.EnemyTypeId;
                break;
            }
        }
        return entryType;
    }

    /// <summary>
    /// �����̐�������G�̃f�[�^�쐬
    /// </summary>
    /// <param name="entryList"></param>
    /// <param name="spawnType"></param>
    /// <param name="canPromoteToElite"></param>
    /// <returns></returns>
    public List<SpawnEnemyData> CreateSpawnEnemyDatas(List<EnemySpawnEntry> entryList, SPAWN_ENEMY_TYPE spawnType, bool canPromoteToElite = true)
    {
        List<SpawnEnemyData> spawnDatas = new List<SpawnEnemyData>();
        foreach (EnemySpawnEntry entry in entryList)
        {
            spawnDatas.Add(CreateSpawnEnemyData(entry, spawnType, canPromoteToElite));
        }
        return spawnDatas;
    }

    /// <summary>
    /// ��������G�̃f�[�^���쐬
    /// </summary>
    /// <param name="enemyPrefabs"></param>
    /// <param name="positions"></param>
    /// <param name="canPromoteToElite"></param>
    /// <returns></returns>
    public SpawnEnemyData CreateSpawnEnemyData(EnemySpawnEntry entryData, SPAWN_ENEMY_TYPE spawnType, bool canPromoteToElite = true)
    {
        if (entryData.EnemyType == null)
        {
            Debug.LogWarning("entryData.EnemyType��null���������߁A�f�[�^�̐����𒆒f���܂����B");
            return null;
        }
        ENEMY_ELITE_TYPE eliteType = ENEMY_ELITE_TYPE.None;

        if (canPromoteToElite && fivePercentOfMaxFloor > eliteEnemyCnt
            && Random.value < spawnProbability)
        {
            eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);

            eliteEnemyCnt++;

            Debug.Log(eliteType);
        }
        else if (canPromoteToElite)
        {
            eliteType = ENEMY_ELITE_TYPE.None;
        }

        // �v���C���[�̌������Z�o
        var playerPos = FetchNearObjectWithTag("Player");
        float scaleX = playerPos.position.x - entryData.Position.x;
        scaleX = scaleX >= 0 ? 1 : -1;
        var enemyScale = new Vector3(scaleX, 1, 1);

        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = Guid.NewGuid(),
            Position = entryData.Position,
            Scale = enemyScale,
            SpawnType = spawnType,
            EliteType = eliteType,
        };
    }


    public SpawnEnemyData CreateTerminalSpawnEnemyData(EnemySpawnEntry entryData, SPAWN_ENEMY_TYPE spawnType)
    {
        if (entryData.EnemyType == null)
        {
            Debug.LogWarning("entryData.EnemyType��null���������߁A�f�[�^�̐����𒆒f���܂����B");
            return null;
        }
        ENEMY_ELITE_TYPE eliteType = (EnumManager.ENEMY_ELITE_TYPE)Random.Range(1, 4);

        Debug.Log(eliteType);

        // �v���C���[�̌������Z�o
        var playerPos = FetchNearObjectWithTag("Player");
        float scaleX = playerPos.position.x - entryData.Position.x;
        scaleX = scaleX >= 0 ? 1 : -1;
        var enemyScale = new Vector3(scaleX, 1, 1);

        return new SpawnEnemyData()
        {
            TypeId = (ENEMY_TYPE)entryData.EnemyType,
            UniqueId = Guid.NewGuid(),
            Position = entryData.Position,
            Scale = enemyScale,
            SpawnType = spawnType,
            EliteType = eliteType,
        };
    }

    /// <summary>
    /// �G�̐������s
    /// </summary>
    /// <param name="spawnEnemyData"></param>
    /// <returns></returns>
    GameObject SpawnEnemy(SpawnEnemyData spawnEnemyData)
    {
        if (spawnEnemyData == null)
        {
            Debug.LogWarning("null�̗v�f�������������߁A�G�̐����𒆒f���܂����B");
            return null;
        }

        // ID�ݒ�(���[�J���p)
        //spawnEnemyData.UniqueId = RoomModel.Instance ? spawnEnemyData.UniqueId : CharacterManager.Instance.Enemies.Count;

        // �G�̐���
        var prefab = idEnemyPrefabPairs[spawnEnemyData.TypeId];
        var position = spawnEnemyData.Position;
        var scale = spawnEnemyData.Scale;
        var eliteType = spawnEnemyData.EliteType;
        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
        //if (LevelManager.Instance.GameLevel > 0)
        //{
        //    enemyObj.GetComponent<CharacterBase>().ApplyStatusModifierByRate(10 * ((int)LevelManager.Instance.GameLevel));
        //}
        enemyObj.transform.localScale = scale;
        enemyObj.GetComponent<EnemyBase>().PromoteToElite(eliteType);
        enemyObj.GetComponent<EnemyBase>().UniqueId = spawnEnemyData.UniqueId;
        CharacterManager.Instance.AddEnemiesToList(new SpawnedEnemy(spawnEnemyData.UniqueId, enemyObj, enemyObj.GetComponent<EnemyBase>(), spawnEnemyData.SpawnType));

        return enemyObj;
    }

    #region ���N�G�X�g�֘A

    /// <summary>
    /// �G�������N�G�X�g
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnEnemyRequest(params SpawnEnemyData[] spawnDatas)
    {
        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatas��null�̗v�f�������������߁A�G�̐����𒆒f���܂����B");
            return;
        }

        // �}�X�^�N���C�A���g�̂ݓG�̐��������N�G�X�g
        if (RoomModel.Instance)
        {
            if (RoomModel.Instance.IsMaster) await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        // ���[�J���p
        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            SpawnEnemy(spawnEnemyData);
        }
    }

    /// <summary>
    /// �^�[�~�i���p�G�������N�G�X�g
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnTerminalEnemyRequest(params SpawnEnemyData[] spawnDatas)
    {
        List<GameObject> enemyObj = new List<GameObject>();

        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatas��null�̗v�f�������������߁A�G�̐����𒆒f���܂����B");
            return;
        }

        // �����œG�̐������N�G�X�g
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // SpawnEnemyAsync���Ăяo��
            await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            enemyObj.Add(SpawnEnemy(spawnEnemyData));
        }
    }

    /// <summary>
    /// �^�[�~�i���p�G�������N�G�X�g
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnTerminalEnemyRequest(Terminal terminal, params SpawnEnemyData[] spawnDatas)
    {
        if (spawnDatas.Any(x => x == null))
        {
            Debug.LogWarning("spawnDatas��null�̗v�f�������������߁A�G�̐����𒆒f���܂����B");
            return;
        }

        // �����œG�̐������N�G�X�g
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // SpawnEnemyAsync���Ăяo��
            await RoomModel.Instance.SpawnEnemyAsync(spawnDatas.ToList());
            return;
        }

        foreach (SpawnEnemyData spawnEnemyData in spawnDatas)
        {
            if (spawnEnemyData == null) continue;
            GameObject enemy = SpawnEnemy(spawnEnemyData);
            enemy.GetComponent<EnemyBase>().TerminalManager = terminal;
            terminal.TerminalSpawnList.Add(enemy);
        }
    }
    #endregion

    #region �ʒm�֘A

    /// <summary>
    /// �G�̐����ʒm
    /// </summary>
    /// <param name="spawnEnemyDatas"></param>
    void OnSpawnEnemy(List<SpawnEnemyData> spawnEnemyDatas)
    {
        foreach(SpawnEnemyData spawnEnemyData in spawnEnemyDatas)
        {
            SpawnEnemy(spawnEnemyData);
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (CharacterManager.Instance && CharacterManager.Instance.PlayerObjSelf)
        {
            var player = CharacterManager.Instance.PlayerObjSelf;
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // �E
            Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // ��
        }
    }

    [ContextMenu("SpawnBoss")]
    public void SpawnBoss()
    {
        if (!isSpawnBoss)
        {
            EnemyBase bossEnemy = idEnemyPrefabPairs[bossId].GetComponent<EnemyBase>();

            SpawnEnemyData spawnEnemyDatas = new SpawnEnemyData();

            int childrenCnt = bossTerminal.transform.childCount;

            List<Transform> children = new List<Transform>();

            for (int i = 0; i < childrenCnt; i++)
            {
                children.Add(bossTerminal.transform.GetChild(i));
            }
            ENEMY_TYPE enemyType = ENEMY_TYPE.Worm;

            Vector3? spawnPos =
                GenerateEnemySpawnPosition(children[0].position, children[1].position, bossEnemy);

            if (spawnPos != null)
            {// �Ԃ�l��null����Ȃ��Ƃ�
                boss = idEnemyPrefabPairs[bossId];
                
                var spawnType = EnumManager.SPAWN_ENEMY_TYPE.ByManager;
                Vector3 scale = Vector3.one;    // ��U���̂܂�
                spawnEnemyDatas = CreateSpawnEnemyData(new EnemySpawnEntry(enemyType, (Vector3)spawnPos, scale), spawnType);
            }

            isSpawnBoss = true;

            UIManager.Instance.DisplayBossUI();

            SpawnEnemyRequest(spawnEnemyDatas);
        }
    }

    /// <summary>
    /// �P�ԋ߂��I�u�W�F�N�g���擾����
    /// </summary>
    /// <param name="tagName">�擾������tagName</param>
    /// <returns>�ŏ������̎w��Obj</returns>
    private Transform FetchNearObjectWithTag(string tagName)
    {
        // �Y���^�O��1���������ꍇ�͂����Ԃ�
        var targets = GameObject.FindGameObjectsWithTag(tagName);
        if (targets.Length == 1) return targets[0].transform;
        GameObject result = null;               // �Ԃ�l
        var minTargetDistance = float.MaxValue; // �ŏ�����
        foreach (var target in targets)
        {
            // �O��v�������I�u�W�F�N�g�����߂��ɂ���΋L�^
            var targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (!(targetDistance < minTargetDistance)) continue;
            minTargetDistance = targetDistance;
            result = target.transform.gameObject;
        }
        // �Ō�ɋL�^���ꂽ�I�u�W�F�N�g��Ԃ�
        return result?.transform;
    }

}