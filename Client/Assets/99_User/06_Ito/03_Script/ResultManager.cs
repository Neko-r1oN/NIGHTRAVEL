using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class ResultManager : MonoBehaviour
{
    [Foldout("テキスト")]
    [SerializeField] Text jobText;                  // 職業
    [Foldout("テキスト")]                           
    [SerializeField] Text levelText;                // ゲームレベル
    [Foldout("テキスト")]                           
    [SerializeField] Text stageNumText;             // 攻略ステージ
    [Foldout("テキスト")]
    [SerializeField] Text arrivalLevelText;         // 到達レベル
    [Foldout("テキスト")]                           
    [SerializeField] Text survivalTimeText;         // 生存時間
    [Foldout("テキスト")]
    [SerializeField] Text totalExterminationText;   // 討伐数
    [Foldout("テキスト")]                           
    [SerializeField] Text grantDamageText;          // 総付与ダメージ
    [Foldout("テキスト")]                           
    [SerializeField] Text receiveDamageText;        // 被ダメージ
    [Foldout("テキスト")]                           
    [SerializeField] Text totalEarningsItemText;    // 総獲得アイテム
    [Foldout("テキスト")]
    [SerializeField] Text terminalStartupNumText;   // 端末起動回数
    [Foldout("テキスト")]
    [SerializeField] Text totalScore;               // 総スコア

    [SerializeField] GameObject ItemImage;
    [SerializeField] Image imagePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //// テストデータ
        //if (!RoomModel.Instance)
        //{
        //    ResultData resultData = new ResultData()
        //    {
        //        PlayerClass = EnumManager.Player_Type.Sword,
        //        GottenRelicList = new List<EnumManager.RELIC_TYPE>() {
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.MoveSpeedTip,
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.CoolingFan},

        //        TotalClearStageCount = 3,
        //        DifficultyLevel = 2,
        //        AliveTime = new TimeSpan(0, 0, 600),
        //        EnemyKillCount = 20,
        //        TotalGaveDamage = 50,
        //        TotalGottenItem = 2,
        //        TotalActivedTerminal = 3,
        //        TotalScore = 30000
        //    };

        //    DisplayResultData(resultData);
        //}
        DisplayResultData(GameManager.Instance.ResultData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// リザルト画面からタイトルに移動
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.DoneFading();
        Initiate.Fade("1_TitleScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// リザルト画面からロビーに移動
    /// </summary>
    public void BackLobbySceneButton()
    {
        Initiate.DoneFading();
        Initiate.Fade("PlayStandbyScene", Color.black, 1.0f);   // フェード時間1秒
    }

    /// <summary>
    /// リザルト画面更新処理
    /// </summary>
    public void DisplayResultData(ResultData resultData)
    {
        jobText.text = resultData.PlayerClass.ToString();   // プレイヤーの情報
        //levelText.text = resultData.DifficultyLevel.ToString();  

        // ゲームの難易度
        if (resultData.DifficultyLevel <= 5)
        {
            levelText.text =
                LevelManager.Instance.LevelName[(DIFFICULTY_TYPE)resultData.DifficultyLevel].ToString();
        }
        else
        {
            levelText.text = LevelManager.Instance.LevelName[DIFFICULTY_TYPE.Hell].ToString();
        }

        List<EnumManager.RELIC_TYPE> relics = new List<EnumManager.RELIC_TYPE>();

        foreach (var item in resultData.GottenRelicList)
        {
            if (!relics.Contains(item))
            {
                GameObject ChildObj =
            Instantiate(imagePrefab.gameObject, Vector3.zero, Quaternion.identity, ItemImage.transform);

                ChildObj.transform.localScale = ChildObj.transform.localScale;

                ChildObj.GetComponent<Image>().sprite =
                RelicManager.Instance.RelicSprites[(int)item - 1];
                relics.Add(item);
            }
        }

        stageNumText.text = resultData.TotalClearStageCount.ToString();           // 攻略ステージ数
        arrivalLevelText.text
            = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().NowLv.ToString();
        survivalTimeText.text = resultData.AliveTime.ToString(@"mm\:ss");         // 生存時間
        totalExterminationText.text = resultData.EnemyKillCount.ToString();       // 総討伐数
        grantDamageText.text = resultData.TotalGaveDamage.ToString();             // 総付与ダメージ数
        totalEarningsItemText.text = resultData.TotalGottenItem.ToString();       // 総獲得アイテム数
        terminalStartupNumText.text = resultData.TotalActivedTerminal.ToString(); // 合計端末起動数
        totalScore.text = resultData.TotalScore.ToString();
    }
}
