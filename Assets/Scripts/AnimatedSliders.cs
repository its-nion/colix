using UnityEngine.UI;
using UnityEngine;

public class AnimatedSliders : MonoBehaviour
{
    public Animator redHandleAnimator;
    public Animator greenHandleAnimator;
    public Animator blueHandleAnimator;


    public void pressRedSlider()
    {
        redHandleAnimator.SetBool("pressed", true);
    }

    public void releaseRedSlider()
    {
        redHandleAnimator.SetBool("pressed", false);
    }
    
    public void pressGreenSlider()
    {
        greenHandleAnimator.SetBool("pressed", true);
    }

    public void releaseGreenSlider()
    {
        greenHandleAnimator.SetBool("pressed", false);
    }
    
    public void pressBlueSlider()
    {
        blueHandleAnimator.SetBool("pressed", true);
    }

    public void releaseBlueSlider()
    {
        blueHandleAnimator.SetBool("pressed", false);
    }

}
