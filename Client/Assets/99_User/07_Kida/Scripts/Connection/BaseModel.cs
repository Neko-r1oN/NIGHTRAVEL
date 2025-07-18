////////////////////////////////////////////////////////////////
///
/// Unityとサーバーの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel : MonoBehaviour
{
    //localhostかAzureのURLを入れる
    public const string ServerURL = "https://localhost:7180";
    /*#if DEBUG

    #else
        //public const string ServerURL = "http://localhost:7000";
        public const string ServerURL = "http://realtime-game.japaneast.cloudapp.azure.com:7000";
    #endif*/
}
