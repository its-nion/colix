using UnityEngine;

public class StartFPS : MonoBehaviour
{

    [Header("SETTINGS")]
    [Range(0, 300)]
    public int i_FrameRate;

    void Awake()
    {
        Application.targetFrameRate = i_FrameRate;
    }

}
