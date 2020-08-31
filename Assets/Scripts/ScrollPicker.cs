
using UnityEngine.UI;
using UnityEngine;

public class ScrollPicker : MonoBehaviour
{
    //Public Variables
    [Header("Settings")]
    [Range(1, 3)] public int colorRGB;
    public float f_SnapSpeed;
    public int i_NumbersDisplayed;
    [Range(0.1f, 10)] public float f_TextDistance;
    [Range(1, 100)] public int i_MaxNumber;

    [Header("Color Transition")]
    public Color32 c_StartColor;
    public Color32 c_EndColor;

    [Header("GameObjects")]
    public RectTransform rt_Panel; // Holds the Scroll Panel
    public RectTransform rt_Center; // Holds my Center to Compare with
    public Text t_Prefab; // Holds the text prefab
    public GameObject colorManager;
    public GameObject firstScroller, secondScroller;

    //Private Variables
    private Text[] t_Texts; // Holds max count of Texts on the Screen
    private float[] f_distanceReposition; //
    private float[] f_distance; // Distance from each Text to the Center to compare with
    private bool b_dragging = false; // True, while we drag the UI Panel
    private int i_CenterButton; // Will hold the button nearest to the center
    private int i_cache = 0; // Saves when button value changes



    private void Start()
    {
        t_Texts = new Text[i_NumbersDisplayed * 2 + 1];

        f_distance = new float[i_NumbersDisplayed * 2 + 1];
        f_distanceReposition = new float[i_NumbersDisplayed * 2 + 1];

        spawnTexts();

        //colorManager.GetComponent<ColorManager>().updateColorPercentage(colorRGB, i_CenterButton);
    }



    private void LateUpdate()
    {

        for (int i = 0; i < i_NumbersDisplayed * 2 + 1; i++)
        {
            if (t_Texts[i] != null)
            {
                f_distanceReposition[i] = rt_Center.GetComponent<RectTransform>().position.y - t_Texts[i].GetComponent<RectTransform>().position.y;
                f_distance[i] = Mathf.Abs(f_distanceReposition[i]);
            }

            else
            {
                f_distanceReposition[i] = 60f;
                f_distance[i] = 60f;
            }

        }

        float f_MinDistance = Mathf.Min(f_distance);

        for (int a = 0; a < i_NumbersDisplayed * 2 + 1; a++)
        {
            if (f_MinDistance == f_distance[a])
            {
                i_CenterButton = int.Parse(t_Texts[a].text);
            }
        }

        if (!b_dragging)
        {
            LerpToButton(-t_Texts[(i_CenterButton % 3)].GetComponent<RectTransform>().anchoredPosition.y);
        }

    }



    private void FixedUpdate()
    {
        if (i_cache != i_CenterButton)
        {
            //colorManager.GetComponent<ColorManager>().updateColorPercentage(colorRGB, i_CenterButton);

            i_cache = i_CenterButton;

            reloadTexts();
        }
    }



    void LerpToButton(float position)
    {
        float newY = Mathf.Lerp(rt_Panel.anchoredPosition.y, position, Time.deltaTime * f_SnapSpeed);
        Vector2 newPosition = new Vector2(rt_Panel.anchoredPosition.x, newY);

        rt_Panel.anchoredPosition = newPosition;
    }



    public void startDrag()
    {
        b_dragging = true;
    }



    public void endDrag()
    {
        b_dragging = false;
    }



    public void spawnTexts()
    {
        for (int i = 0; i <= 1; i++)
        {
            Vector3 spawnPos = new Vector3(rt_Center.position.x, rt_Center.position.y + (i * f_TextDistance), rt_Center.position.z);
            t_Texts[i] = Instantiate(t_Prefab, spawnPos, Quaternion.identity, rt_Panel);
            t_Texts[i].rectTransform.position = spawnPos;
            t_Texts[i].text = i.ToString();
        }
    }



    public void reloadTexts()
    {
        var x = (i_CenterButton + i_NumbersDisplayed) % 3;
        var y = (i_CenterButton - i_NumbersDisplayed) % 3;

        if (i_CenterButton >= 1 && i_CenterButton <= i_MaxNumber - 1) // falls aktueller wert zwischen 1 und maxzahl -1 liegt | i_MaxNumber - i_NumbersDisplayed
        {
            if (t_Texts[x] != null) // loescht oberhalb des aktuellen texts
            {
                Destroy(t_Texts[x].gameObject);
            }

            if (t_Texts[y] != null) // loescht unterhalb des aktuellen texts
            {
                Destroy(t_Texts[y].gameObject);
            }

            Vector3 spawnPosTop = new Vector3(t_Texts[i_CenterButton % 3].transform.position.x, t_Texts[i_CenterButton % 3].transform.position.y + f_TextDistance, rt_Center.position.z); // instantiated oberen button
            t_Texts[x] = Instantiate(t_Prefab, spawnPosTop, Quaternion.identity, rt_Panel);
            t_Texts[x].rectTransform.position = spawnPosTop;
            t_Texts[x].text = (i_CenterButton + 1).ToString();

            Vector3 spawnPosBot = new Vector3(t_Texts[i_CenterButton % 3].transform.position.x, t_Texts[i_CenterButton % 3].transform.position.y - f_TextDistance, rt_Center.position.z); // instantiated unteren button
            t_Texts[y] = Instantiate(t_Prefab, spawnPosBot, Quaternion.identity, rt_Panel);
            t_Texts[y].rectTransform.position = spawnPosBot;
            t_Texts[y].text = (i_CenterButton - 1).ToString();
        }
    }

}
