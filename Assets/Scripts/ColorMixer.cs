using UnityEngine.UI;
using UnityEngine;

public class ColorMixer : MonoBehaviour
{
    [Header("currentColors")]
    public Color currentColor;
    public Color goalColor;

    [Range(0, 25f)] public float maxColorDifference = 10f;

    public bool hex;

    [Header("GameObjects")]
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    public Text hashtagText;
    public Text redText;
    public Text greenText;
    public Text blueText;

    public SpriteRenderer goalColorSprite;

    public Animator everything;

    public GameObject wave1, wave2, wave3;

    private float currentcolorsumred, goalcolorsumred, currentcolorsumgreen, goalcolorsumgreen, currentcolorsumblue, goalcolorsumblue;

    //--------------------------------------------- PROGRESS BAR -----------------------------------------------------------------------------------------------

    public void onRedSliderChange()
    {
        currentColor.r = redSlider.value;

        refreshcurrentColor();
        refreshText();
    }

    public void onGreenSliderChange()
    {
        currentColor.g = greenSlider.value;

        refreshcurrentColor();
        refreshText();
    }

    public void onBlueSliderChange()
    {
        currentColor.b = blueSlider.value;

        refreshcurrentColor();
        refreshText();
    }

    public void checkIfcurrentColorAchieved()
    {
        calculateColorSums();

        if((Mathf.Abs(currentcolorsumred - goalcolorsumred) <= maxColorDifference) && Mathf.Abs(currentcolorsumgreen - goalcolorsumgreen) <= maxColorDifference && Mathf.Abs(currentcolorsumblue - goalcolorsumblue) <= maxColorDifference)
        {
            everything.SetBool("sucess", true);
        }
    }

    //--------------------------------------------- HEX TEXT SHOWN -----------------------------------------------------------------------------------------------

    public void onHexChange()
    {
        if (hex)
        {
            hex = false;
            hashtagText.text = "";
            refreshText();
        }
        else
        {
            hex = true;
            hashtagText.text = "#";
            refreshText();
        }
    }

    //------------------------------------ START FUNCTION --------------------------------------------------------------------------------------------------------

    private void Start()
    {
        currentColor.r = redSlider.value;
        currentColor.g = greenSlider.value;
        currentColor.b = blueSlider.value;

        calculateColorSums();

        refreshgoalColor();
        refreshcurrentColor();

        refreshText();
    }

    //------------------------------------- LOCAL FUNCTIONS -------------------------------------------------------------------------------------------------------

    private void refreshcurrentColor()
    {
        currentColor.a = 1;
        wave1.GetComponent<WaveMesh>().cWaveColor = currentColor;
        currentColor.a = 0.60f;
        wave2.GetComponent<WaveMesh>().cWaveColor = currentColor;
        currentColor.a = 0.41f;
        wave3.GetComponent<WaveMesh>().cWaveColor = currentColor;
    }

    private void refreshgoalColor()
    {
        goalColorSprite.color = goalColor;
    }

    private void refreshText()
    {
        if (hex)
        {
            redText.text = ((int)(currentColor.r * 255)).ToString("X");
            greenText.text = ((int)(currentColor.g * 255)).ToString("X");
            blueText.text = ((int)(currentColor.b * 255)).ToString("X");
        }
        else
        {
            redText.text = ((int)(currentColor.r * 255)).ToString();
            greenText.text = ((int)(currentColor.g * 255)).ToString();
            blueText.text = ((int)(currentColor.b * 255)).ToString();
        }
    }

    private void calculateColorSums()
    {
        currentcolorsumred = ((int)(currentColor.r * 255));
        goalcolorsumred = ((int)(goalColor.r * 255));

        currentcolorsumgreen = ((int)(currentColor.g * 255));
        goalcolorsumgreen = ((int)(goalColor.g * 255));

        currentcolorsumblue = ((int)(currentColor.b * 255));
        goalcolorsumblue = ((int)(goalColor.b * 255));
    }

}
