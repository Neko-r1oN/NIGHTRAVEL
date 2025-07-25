using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared;

public class CharacterManager : MonoBehaviour
{
    #region �v���C���[�֘A
    [SerializeField] GameObject charaSwordPrefab;
    #endregion

    #region �G�֘A
    Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
    #endregion

    const float updateSec = 0.1f;
    bool isAwake = false;

    private void Awake()
    {
        if (RoomModel.Instance.ConnectionId == Guid.Empty) return;
        isAwake = true;
        StartCoroutine("UpdateCoroutine");
    }

    /// <summary>
    /// �L�����N�^�[�̏��X�V�Ăяo���p�R���[�`��
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        if (RoomModel.Instance.IsMaster) UpdateMasterData();
        else UpdatePlayerData();
        yield return new WaitForSeconds(updateSec);
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�p�̏��X�V
    /// </summary>
    async void UpdateMasterData()
    {

    }

    /// <summary>
    /// �v���C���[�̏��X�V
    /// </summary>
    async void UpdatePlayerData()
    {

    }

    /// <summary>
    /// �V���ȓG�����X�g�ɒǉ�����
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemies(params GameObject[] newEnemies)
    {
        foreach (GameObject enemy in newEnemies)
        {
            enemies.Add(enemies.Count, enemy);
        }
    }
}
