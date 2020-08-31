using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RepositionSprite : MonoBehaviour
{

    public bool b_TopOrBotSprite;
    public float scale;
    private Sprite sp_Sprite;

    void Start()
    {

        sp_Sprite = GetComponent<SpriteRenderer>().sprite;
        Vector3 v3_ScreenSize = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));

        var scaleFactorX = v3_ScreenSize.x / sp_Sprite.rect.width;
        var scaleFactorY = v3_ScreenSize.y / sp_Sprite.rect.height;

        if (b_TopOrBotSprite)
        {
            transform.localPosition = new Vector3(0, v3_ScreenSize.y / 2, 0);

            transform.localScale = new Vector3(scaleFactorX * 25, scaleFactorY * scale, 1);
        }

        else
        {
            transform.localPosition = new Vector3(0, -v3_ScreenSize.y / 2, 0);

            transform.localScale = new Vector3(scaleFactorX * 25, scaleFactorY * scale, 1);
        }

    }

}
