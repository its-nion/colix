using UnityEngine;

[ExecuteInEditMode]
public class MobilePostProcessing : MonoBehaviour
{
    public bool Blur = false;
    [Range(0, 1)]
    public float BlurAmount = 1f;
    [Range(1.0f, 8.0f)]
    public float BlurDownScaling = 1f;
    [Range(0.01f, 3.0f)]
    public float BlurDownScalingSmoothness = 1f;
    public Texture2D BlurMask;
    [Range(1,5)]
    public int NumberOfPasses = 3;
    public bool Bloom = false;
    public Color BloomColor = Color.white;
    [Range(0, 5)]
    public float BloomAmount = 1f;
    [Range(0, 1)]
    public float BloomThreshold = 0f;

    public bool LUT = false;
    [Range(2, 3)]
    public int LutDimension = 2;
    [Range(0, 1)]
    public float LutAmount = 0.0f;
    public Texture2D SourceLut = null;

    public bool ImageFiltering = false;
    [Range(0, 1)]
    public float Contrast = 0f;
    [Range(-1, 1)]
    public float Brightness = 0f;
    [Range(-1, 1)]
    public float Saturation = 0f;
    [Range(-1, 1)]
    public float Exposure = 0f;
    [Range(-1, 1)]
    public float Gamma = 0f;

    public bool ChromaticAberration = false;
    [Range(0, 10)]
    public float Offset = 0;

    public bool Vignette = false;
    [Range(0, 1)]
    public float VignetteValue = 1f;

    static readonly int blurTexString = Shader.PropertyToID("_BlurTex");
    static readonly int maskTextureString = Shader.PropertyToID("_MaskTex");
    static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
    static readonly int colorString = Shader.PropertyToID("_Color");
    static readonly int blAmountString = Shader.PropertyToID("_BloomAmount");
    static readonly int blThresholdString = Shader.PropertyToID("_BloomThreshold");
    static readonly int lutTexture2DString = Shader.PropertyToID("_LutTex2D");
    static readonly int lutTexture3DString = Shader.PropertyToID("_LutTex3D");
    static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");
    static readonly int contrastString = Shader.PropertyToID("_Contrast");
    static readonly int brightnessString = Shader.PropertyToID("_Brightness");
    static readonly int saturationString = Shader.PropertyToID("_Saturation");
    static readonly int exposureString = Shader.PropertyToID("_Exposure");
    static readonly int gammaString = Shader.PropertyToID("_Gamma");
    static readonly int offsetString = Shader.PropertyToID("_Offset");
    static readonly int vignetteString = Shader.PropertyToID("_Vignette");

    static readonly string bloomKeyword = "BLOOM";
    static readonly string blurKeyword = "BLUR";
    static readonly string chromaKeyword = "CHROMA";
    static readonly string lutKeyword = "LUT";
    static readonly string filterKeyword = "FILTER";

    public Material material;

    private int previousLutDimension;
    private Texture2D previous;
    private Texture2D converted2D = null;
    private Texture3D converted3D = null;

    public void Start()
    {
        previousLutDimension = LutDimension;

        if (BlurMask==null)
        {
            Shader.SetGlobalTexture(maskTextureString, Texture2D.whiteTexture);
        }
        else
            Shader.SetGlobalTexture(maskTextureString, BlurMask);
    }

    public void Update()
    {
        if (previousLutDimension != LutDimension)
        {
            previousLutDimension = LutDimension;
            Convert(SourceLut);
            return;
        }

        if (SourceLut != previous)
        {
            previous = SourceLut;
            Convert(SourceLut);
        }
    }

    private void OnDestroy()
    {
        if (converted2D != null)
        {
            DestroyImmediate(converted2D);
        }
        converted2D = null;
    }

    private void Convert2D(Texture2D temp2DTex)
    {
        Color[] color = temp2DTex.GetPixels();
        Color[] newCol = new Color[65536];

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                for (int x = 0; x < 16; x++)
                    for (int y = 0; y < 16; y++)
                    {
                        float bChannel = (i + j * 16.0f) / 16;
                        int bchIndex0 = Mathf.FloorToInt(bChannel);
                        int bchIndex1 = Mathf.Min(bchIndex0 + 1, 15);
                        float lerpFactor = bChannel - bchIndex0;
                        int index = x + (15 - y) * 256;
                        Color col1 = color[index + bchIndex0 * 16];
                        Color col2 = color[index + bchIndex1 * 16];

                        newCol[x + i * 16 + y * 256 + j * 4096] =
                            Color.Lerp(col1, col2, lerpFactor);
                    }

        if (converted2D)
            DestroyImmediate(converted2D);
        converted2D = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        converted2D.SetPixels(newCol);
        converted2D.Apply();
        converted2D.wrapMode = TextureWrapMode.Clamp;
    }


    private void Convert3D(Texture2D temp3DTex)
    {
        var color = temp3DTex.GetPixels();
        var newCol = new Color[color.Length];

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    int val = 16 - j - 1;
                    newCol[i + (j * 16) + (k * 256)] = color[k * 16 + i + val * 256];
                }
            }
        }
        if (converted3D)
            DestroyImmediate(converted3D);
        converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, false);
        converted3D.SetPixels(newCol);
        converted3D.Apply();
        converted3D.wrapMode = TextureWrapMode.Clamp;
    }

    private void Convert(Texture2D source)
    {
        if (LutDimension == 2)
        {
            Convert2D(source);
        }
        else
        {
            Convert3D(source);
        }
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Blur || Bloom)
        {
            material.SetFloat(blurAmountString, BlurAmount * 2.5f);
            material.EnableKeyword(blurKeyword);
            material.DisableKeyword(bloomKeyword);

            if (Bloom)
            {
                material.EnableKeyword(bloomKeyword);
                material.DisableKeyword(blurKeyword);
                material.SetFloat(blAmountString, BloomAmount + 1f);
                material.SetColor(colorString, BloomColor);
                material.SetFloat(blThresholdString, BloomThreshold);
            }

            if (BlurAmount > 0)
            {
                RenderTexture blurTex = null;

                if (NumberOfPasses == 1)
                {
                    blurTex = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), 0, source.format);
                    Graphics.Blit(source, blurTex, material, 0);
                }
                if (NumberOfPasses == 2)
                {
                    blurTex = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), 0, source.format);
                    var temp1 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), 0, source.format);
                    Graphics.Blit(source, temp1, material, Bloom ? 2 : 1);
                    Graphics.Blit(temp1, blurTex, material, 1);
                    RenderTexture.ReleaseTemporary(temp1);
                }
                else if (NumberOfPasses == 3)
                {
                    blurTex = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), 0, source.format);
                    var temp1 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), 0, source.format);
                    Graphics.Blit(source, blurTex, material, Bloom ? 2 : 1);
                    Graphics.Blit(blurTex, temp1, material, 1);
                    Graphics.Blit(temp1, blurTex, material, 1);
                    RenderTexture.ReleaseTemporary(temp1);
                }              
                else if (NumberOfPasses == 4)
                {
                    blurTex = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), 0, source.format);
                    var temp1 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 4 - ((BlurDownScaling * 4) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 4 - ((BlurDownScaling * 4) % BlurDownScalingSmoothness))), 0, source.format);
                    var temp2 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), 0, source.format);
                    Graphics.Blit(source, temp2, material, Bloom ? 2 : 1);
                    Graphics.Blit(temp2, blurTex, material, 1);
                    Graphics.Blit(blurTex, temp1, material, 1);
                    Graphics.Blit(temp1, blurTex, material, 1);
                    RenderTexture.ReleaseTemporary(temp1);
                    RenderTexture.ReleaseTemporary(temp2);
                }
                else if (NumberOfPasses == 5)
                {
                    blurTex = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling - (BlurDownScaling % BlurDownScalingSmoothness))), 0, source.format);
                    var temp1 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 2 - ((BlurDownScaling * 2) % BlurDownScalingSmoothness))), 0, source.format);
                    var temp2 = RenderTexture.GetTemporary((int)(Screen.width / (BlurDownScaling * 4 - ((BlurDownScaling * 4) % BlurDownScalingSmoothness))), (int)(Screen.height / (BlurDownScaling * 4 - ((BlurDownScaling * 4) % BlurDownScalingSmoothness))), 0, source.format);
                    Graphics.Blit(source, blurTex, material, Bloom ? 2 : 1);
                    Graphics.Blit(blurTex, temp1, material, 1);
                    Graphics.Blit(temp1, temp2, material, 1);
                    Graphics.Blit(temp2, temp1, material, 1);
                    Graphics.Blit(temp1, blurTex, material, 1);
                    RenderTexture.ReleaseTemporary(temp1);
                    RenderTexture.ReleaseTemporary(temp2);
                }

                material.SetTexture(blurTexString, blurTex);
                RenderTexture.ReleaseTemporary(blurTex);
            }
            else
            {
                material.SetTexture(blurTexString, source);
            }
        }
        else
        {
            material.DisableKeyword(blurKeyword);
            material.DisableKeyword(bloomKeyword);
        }

        if (LUT)
        {
            material.EnableKeyword(lutKeyword);
            material.SetFloat(lutAmountString, LutAmount);

            if (LutDimension == 2)
            {
                material.SetTexture(lutTexture2DString, converted2D);
            }
            else
            {
                material.SetTexture(lutTexture3DString, converted3D);
            }
        }
        else
        {
            material.DisableKeyword(lutKeyword);
        }

        if (ImageFiltering)
        {
            material.EnableKeyword(filterKeyword);
            material.SetFloat(contrastString, Contrast + 1f);
            material.SetFloat(brightnessString, Brightness * 0.5f + 0.5f);
            material.SetFloat(saturationString, Saturation + 1f);
            material.SetFloat(exposureString, Exposure);
            material.SetFloat(gammaString, Gamma);
        }
        else
        {
            material.DisableKeyword(filterKeyword);
        }

        if (ChromaticAberration)
        {
            material.EnableKeyword(chromaKeyword);
            material.SetFloat(offsetString, Offset);
        }
        else
        {
            material.DisableKeyword(chromaKeyword);
        }

        if (Vignette)
        {
            material.SetFloat(vignetteString, VignetteValue * 2.5f);
        }
        else
        {
            material.SetFloat(vignetteString, 0f);
        }

        Graphics.Blit(source, destination, material, LutDimension+1);
    }
}
