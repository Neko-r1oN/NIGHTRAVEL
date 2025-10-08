using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    RectTransform rectTransform = null;

    [SerializeField] public Transform target = null;

    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] float rate_pos = 10.0f;
    [SerializeField] float rate_offset = 2000.0f;
    [SerializeField] float rate_scale = 4.5f;

    public float offsetMinY = 200.0f;
    public float offsetMaxY = 200.0f;

    public float scaleMin = 0.5f;
    public float scaleMax = 1.5f;

    public Vector2 edgeBuffer = Vector2.zero;

    ///--------------------方向矢印----------------------
    [SerializeField]  bool arrow = false;
    public GameObject arrowUI;
    public GameObject nameUI;
    RectTransform rect_arrow = null;
    RectTransform rect_name = null;
    ///--------------------------------------------------

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        rectTransform.localScale = Vector3.one;
        if(arrowUI)rect_arrow = arrowUI.GetComponent<RectTransform>();
    }
    private void Update()
    {
        ///----------------オフセット(画面外余白)処理---------------------------
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        if(screenPos.z < 0f)
        {
            screenPos *= -1f;
        }

        Vector3 toTaget = target.position - Camera.main.transform.position;
        float distance = toTaget.magnitude;

        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY,offsetMinY,offsetMaxY);

        if (!arrow) PlayerName(screenPos/*, distance*/);
        else PlayerNameAndArrow(screenPos);
        ///---------------------------------------------------------------------
    }
    /// <summary>
    /// プレイヤー名UI座標移動処理
    /// </summary>
    /// <param name="screenPos">スクリーン座標</param>
    /// <param name="distance">距離スケール拡縮</param>
    void PlayerName(Vector3 screenPos/*,float distance*/)
    {
        Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);

        float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
        float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.x);

        Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);


        ///距離サイズ調整用(今回は2Dなので使わない)
        /*
        if(distance < 10.0f && distance > 5.0f)
        {
            Vector3 targetScale = Vector3.one / distance * rate_scale;
            targetScale.x = Mathf.Clamp(targetScale.x,scaleMin,scaleMax);
            targetScale.y = Mathf.Clamp(targetScale.y,scaleMin,scaleMax);
            targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale,targetScale, Time.deltaTime * 10.0f);
        }
        else if(distance <= 5.0f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
        }
        else if (distance >= 10.0f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
        }7*/

        rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

    }

    /// <summary>
    /// 描画UI切替処理
    /// </summary>
    /// <param name="screenPos"></param>
    /// <param name="distance"></param>
    void PlayerNameAndArrow(Vector3 screenPos/*, float distance*/)
    {
        bool isBehindCamera = screenPos.z < 0f;
        if(isBehindCamera)
        {
            screenPos *= -1.0f;
        }

        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
            screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;  //対象画面外判定

        if (isOffScreen)
        {
            arrowUI.SetActive(true);
            nameUI.SetActive(false);

            ///距離サイズ調整用(今回は2Dなので使わない)

            /* if (distance < 10.0f && distance > 5.0f)
             {
                 Vector3 targetScale = Vector3.one / distance * rate_scale;
                 targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
                 targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
                 targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
                 rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
             }
             else if (distance <= 5.0f)
             {
                 rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
             }
             else if (distance >= 10.0f)
             {
                 rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
             }*/


            /*
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0.0f) * 0.5f;
            Vector3 dir = (screenPos - screenCenter).normalized;

            Vector3 arrowScreenPos = screenCenter + dir * ((Mathf.Min(Screen.width, Screen.height) * 0.5f) - edgeBuffer.magnitude);
            arrowScreenPos.x = Mathf.Clamp(arrowScreenPos.x,edgeBuffer.x, Screen.width - edgeBuffer.x);
            arrowScreenPos.y = Mathf.Clamp(arrowScreenPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);


            rectTransform.position = arrowScreenPos;
            */

            Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);

            float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.x);

            Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);

            rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);
            GameObject arrow = this.transform.GetChild(1).transform.GetChild(0).gameObject;

            //float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
            float angle = Mathf.Atan2(desiredUIPos.y, desiredUIPos.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            
        }
        else
        {
            arrowUI.SetActive(false);
            nameUI.SetActive(true);

            ///距離サイズ調整用(今回は2Dなので使わない)
            /*
            if (distance < 10.0f && distance > 5.0f)
            {
                Vector3 targetScale = Vector3.one / distance * rate_scale;
                targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
                targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
                targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
            }
            else if (distance <= 5.0f)
            {
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
            }
            else if (distance >= 10.0f)
            {
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
            }*/

            Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
            float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width -  edgeBuffer.x);
            float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);
            rectTransform.position = Vector3.Lerp(rectTransform.position,targetUIPos,Time.deltaTime * rate_pos);

            rectTransform.rotation = Quaternion.identity;
        }
    }
}
