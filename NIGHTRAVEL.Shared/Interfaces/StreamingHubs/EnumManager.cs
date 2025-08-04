//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnumManager
    {

        #region システム関連
        /// <summary>
        /// タイマ-の種類
        /// Author:Nishiura
        /// </summary>
        public enum TIME_TYPE
        {
            GameTimer,          // ゲーム全体のタイマー
            StageClearTimer,    // ステージクリア後待機タイマー
            TerminalTimer       // 端末のレース用タイマー
        }

        /// <summary>
        /// 難易度の種類
        /// Author:Nishiura
        /// </summary>
        public enum DIFFICULTY_TYPE
        {
            Baby,
            Easy,
            Noraml,
            Hard,
            VeryHard,
            Hell
        }
        #endregion

        #region キャラクター関連

        /// <summary>
        /// 状態異常の種類
        /// </summary>
        public enum DEBUFF_TYPE
        {
            None = 0,
            Burn,       // 炎上状態
            Freeze,     // 霜焼け状態
            Shock       // 感電状態
        }

        /// <summary>
        /// ステータス強化の選択肢の種類
        /// DB上のレコードのIDと完全一致させる
        /// </summary>
        public enum STAT_UPGRADE_OPTION 
        {
            Sample
        }

        /// <summary>
        /// ステータスの種類
        /// </summary>
        public enum STATUS_TYPE
        {
            All,
            HP,
            Defense,
            Power,
            JumpPower,
            MoveSpeed,
            MoveSpeedFactor,
            AttackSpeedFactor
        }

        #region 操作キャラ関連

        /// <summary>
        /// 操作キャラの種類
        /// </summary>
        public enum Player_Type
        {
            Sword,
            Gunner
        }

        #endregion
        #region 敵関連

        /// <summary>
        /// 敵の種類
        /// </summary>
        public enum ENEMY_TYPE
        {
            CyberDog = 0,
            Drone,
            CyberDog_ByWorm,
            Drone_ByWorm,
            Worm,
        }

        /// <summary>
        /// 敵の生成タイプ
        /// </summary>
        public enum SPAWN_ENEMY_TYPE
        {
            ByManager,      // Managerによる生成
            ByTerminal,     // 端末による生成
            ByWorm,         // ワームによる生成
        }

        /// <summary>
        /// エリートの種類
        /// </summary>
        public enum ENEMY_ELITE_TYPE
        {
            None,
            Blaze,      // ブレイズエリート
            Frost,      // フロストエリート
            Thunder     // サンダーエリート
        }

        #endregion

        #endregion
    }
}
