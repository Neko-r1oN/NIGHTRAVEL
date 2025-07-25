//----------------------------------------------------
// �G�����N���X
// Author : Souma Ueno
//----------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region �G��������
    [SerializeField] Vector2 spawnRange;
    [SerializeField] float spawnRangeOffset;
    #endregion

    #region �X�e�[�W���
    [SerializeField] Transform stageMin;             // ���X�|�[���͈�A
    [SerializeField] Transform stageMax;             // ���X�|�[���͈�B
    //[SerializeField] Transform minTerminalRespawn;       // �^�[�~�i�����X�|�[���͈�
    //[SerializeField] Transform maxTerminalRespawn;       // �^�[�~�i�����X�|�[���͈�

    #region �O���Q��
    public Transform StageMinPoint { get { return stageMin; } }
    public Transform StageMaxPoint { get { return stageMax; } }
    #endregion

    #endregion

    #region �G�֘A
    [SerializeField] List<GameObject> enemyPrefabs;      // �G�l�~�[���X�g
    [SerializeField] List<GameObject> enemiesByTerminal; // �[�����琶�����ꂽ�G�̃��X�g
    float[] enemyWeights;
    int eliteEnemyCnt;

    #region �O���Q��
    public List<GameObject> EnemiesByTerminal { get { return enemiesByTerminal; } }
    #endregion

    #endregion

    #region TMP
    [SerializeField] float distMinSpawnPos;              // �������Ȃ��͈�
    [SerializeField] float xRadius;                      // �����͈͂�x���a
    [SerializeField] float yRadius;                      // �����͈͂�y���a
    #endregion

    [SerializeField] GameObject player;
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
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        enemyWeights = new float[enemyPrefabs.Count];

        // ����
        enemyWeights[0] = 24; // �h���[��
        enemyWeights[1] = 76; // ����
    }

    /// <summary>
    /// �G�̃X�|�[���\�͈͔��菈��
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    public (Vector3 minRange, Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < stageMin.position.y)
        {
            minRange.y = stageMin.position.y;
        }

        if (minPoint.x < stageMin.position.x)
        {
            minRange.x = stageMin.position.x;
        }

        if (maxPoint.y > stageMax.position.y)
        {
            maxRange.y = stageMax.position.y;
        }

        if (maxPoint.x > stageMax.position.x)
        {
            maxRange.x = stageMax.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// �G�̃X�|�[���\�͈͔��菈��
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    /*public (Vector3 minRange, Vector3 maxRange) CreateEnemyTerminalSpawnPosition(Vector3 minPoint, Vector3 maxPoint)
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
    }*/

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
            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector3 distToPlayer =
                player.transform.position - spawnPos;

            if (Mathf.Abs(distToPlayer.x) > distMinSpawnPos
                && Mathf.Abs(distToPlayer.y) > distMinSpawnPos)
            {
                Vector2? pos = IsGroundCheck(spawnPos);
                if (pos != null)
                {
                    LayerMask mask = LayerMask.GetMask("Default");

                    Vector2 result = (Vector2)pos;

                    result.y += enemyBase.SpawnGroundOffset;

                    if (!Physics2D.OverlapCircle(new Vector2(result.x, result.y + 1), 0.8f, mask))
                    {
                        return result;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// �G��������
    /// </summary>
    public void GenerateEnemy(int num)
    {
        for (int i = 0; i < num; i++)
        {
            int seed = System.DateTime.Now.Millisecond + i;
            Random.InitState(seed);  // Unity�̗����ɃV�[�h��ݒ�

            // �m���̌v�Z
            int listNum = Choose(enemyWeights);

            EnemyBase enemyBase = enemyPrefabs[listNum].GetComponent<EnemyBase>();

            // �����_���Ȉʒu�𐶐�
            //var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

            //Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            //if (spawnPos != null)
            //{
            //    SpawnEnemyRequest(enemyPrefabs[listNum], (Vector3)spawnPos);
            //}
        }
    }

    /// <summary>
    /// �[�����쎞�̓G��������
    /// </summary>
    public void TerminalGenerateEnemy(int num)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
        {
            int listNum = Random.Range(0, enemyPrefabs.Count);

            EnemyBase enemyBase = enemyPrefabs[listNum].GetComponent<EnemyBase>();

            Vector2 minPlayer =
                        new Vector2(player.transform.position.x - xRadius,
                        player.transform.position.y - yRadius);

            Vector2 maxPlayer =
                new Vector2(player.transform.position.x + xRadius,
                player.transform.position.y + yRadius);

            // �����_���Ȉʒu�𐶐�
            /*var spawnPostions = CreateEnemyTerminalSpawnPosition(minPlayer, maxPlayer);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                SpawnEnemyRequest(enemyPrefabs[listNum], (Vector3)spawnPos);

                // �[������o���G�����X�g�ɒǉ�
                enemiesByTerminal.Add(enemy);

                enemyCnt++;
            }*/
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
    /// �G�������N�G�X�g
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public GameObject SpawnEnemyRequest(GameObject spawnEnemy,Vector3 spawnPos, bool canPromoteToElite = true)
    {
        GameManager.Instance.SpawnCnt++;

        // ����
        GameObject enemy = Instantiate(spawnEnemy, spawnPos, Quaternion.identity);
        if (!canPromoteToElite) return enemy;

        int number = Random.Range(0, 100);

        // �G���[�g�G�������x��ݒ�
        int onePercentOfMaxEnemies =
            Mathf.FloorToInt(GameManager.Instance.MaxSpawnCnt * 0.3f);

        if (number < 5 * ((int)LevelManager.Instance.GameLevel + 1) 
            && eliteEnemyCnt < onePercentOfMaxEnemies)
        {// 5%(* ���݂̃Q�[�����x��)�ȉ��ŁA�G���[�g�G�������x�ɒB���Ă��Ȃ�������
            // �G���[�g�G����
            enemy.GetComponent<EnemyBase>().PromoteToElite((EnemyElite.ELITE_TYPE)Random.Range(1, 4));
            eliteEnemyCnt++;
        }

        return enemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // �E
        Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // ��
    }
}
