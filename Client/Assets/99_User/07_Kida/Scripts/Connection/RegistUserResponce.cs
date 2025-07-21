////////////////////////////////////////////////////////////////
///
/// 送信情報を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////


using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistUserResponse
{
    [JsonProperty("user_id")]
    public int UserID { get; set; }

    [JsonProperty("token")]
    public string Authtoken { get; set; }
}
