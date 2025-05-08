////////////////////////////////////////////////////////////////////////////
///
///  ユーザーステータススクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Model.Entity
{
    /// <summary>
    /// ユーザー状態クラス
    /// </summary>
    [MessagePackObject]
    public class UserState
    {
        [Key(0)]
        public bool isReady { get; set; }                    //準備状態
        [Key(1)]
        public bool isGameCountFinish { get; set; }          //カウントダウンが終了しているか
        [Key(2)]
        public bool isGameFinish { get; set; }               //ゲームが終了しているか
        [Key(3)]
        public int Ranking { get; set; }                     //順位
        [Key(4)]
        public int Score { get; set; }                       //獲得スコア
        [Key(5)]
        public int Life { get; set; }                       //体力
        [Key(6)]
        public int UseCharaId { get; set; }                  //使用キャラクターID
        [Key(7)]
        public bool isHaveBall { get; set; }                  //ボール所持状況

    }
}