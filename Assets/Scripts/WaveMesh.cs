using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class WaveMesh : MonoBehaviour
{

    [Header("General")] // -------------------------------------------------------------------------------------------------------------

    public Color cWaveColor;

    [Range(1, 100)]
    public int iWaveResolution = 10; //Aus wie vielen Punkten die Meshwave besteht

    [Range(0, 10)]
    public float fWaveSpeed = 1; // Schnelligkeit der Wellen

    [Range(0, 10)]
    public float fWaveHeight = 1; // Die hoehe der Wellen (nicht abhaengig von dem Abstand der Wellen zur Mitte)

    [Range(0, 50)]
    public float fWaveLength = 0.001f; // Wie viele Wellen in den Vorgegebenen Bereich passen

    [Header("Top Wave")] // -------------------------------------------------------------------------------------------------------------

    [Range(-2, 25)]
    public float fTopWavePosition = 1.25f; // Wie sehr beide Wellen verschoben sind

    [Range(0, 25)]
    public float fTopWaveOffset = 1.25f; // Wie sehr beide Wellen verschoben sind

    [Header("Bot Wave")] // -------------------------------------------------------------------------------------------------------------

    [Range(-2, 25)]
    public float fBottomWavePosition = 1.25f; // Wie sehr die untere Welle verschoben ist

    [Range(0, 25)]
    public float fBottomWaveOffset = 1.25f; // Wie sehr die untere Welle verschoben ist

    [Header("Meshes")] // -------------------------------------------------------------------------------------------------------------

    public MeshFilter mfMidWaveMeshFilter;

    [Header("Camera")] // -------------------------------------------------------------------------------------------------------------

    public Camera cMainCamera;

    // PRIVATE STUFF -------------------------------------------------------------------------------------------------------------------------   

    private float fScreenWidth;

    private float fScreenHeigh;

    private Vector3 v3WaveSpawnPosition;

    private int iWavePart = 1;

    private bool bDrawFirstRectangle = true;
    
    private float fTopPosition = 1; // Die y-Position der oberen Welle (aendert nichts an der hoehe der Wellen)
    
    private float fBottomPosition = 1; // Die y-Position der unteren Welle (aendert nichts an der hoehe der Wellen)



    void Start()
    {

        getScreenWidthAndHeigh();

        buildMidWaveMesh();

    }



    void Update()
    {

        fTopPosition = (fScreenHeigh / 2) + fTopWavePosition;

        fBottomPosition = 0 + fBottomWavePosition;

        refreshMidWaveMesh();

    }



    void getScreenWidthAndHeigh()
    {

        fScreenWidth = cMainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        fScreenHeigh = cMainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;

    }



    void buildMidWaveMesh()
    {

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // CREATE VERTICES AT GIVEN POINTS -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        var mesh = new Mesh();
        mfMidWaveMeshFilter.mesh = mesh;

        var vertices = new Vector3[4 + (iWaveResolution * 2)];

        vertices[0] = new Vector3(-fScreenWidth, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed) * fWaveHeight, 0);       // ERSTER VERTEX LINKS OBEN 
        vertices[1] = new Vector3(-fScreenWidth, -fBottomPosition - Mathf.Sin((Time.time + fBottomWaveOffset) * fWaveSpeed) * fWaveHeight, 0);   // ZWEITER VERTEX LINKS UNTEN 

        vertices[iWaveResolution * 2 + 2] = new Vector3(fScreenWidth, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed + ((2 * iWaveResolution + 1) / fWaveLength)) * fWaveHeight, 0);       // ERSTER VERTEX RECHTS OBEN 
        vertices[iWaveResolution * 2 + 3] = new Vector3(fScreenWidth, -fBottomPosition - Mathf.Sin((Time.time + fBottomWaveOffset) * fWaveSpeed + ((2 * iWaveResolution + 2) / fWaveLength)) * fWaveHeight, 0);   // ZWEITER VERTEX RECHTS UNTEN 

        for (int i = 2; i <= iWaveResolution * 2; i += 2)
        {
            vertices[i] = new Vector3(-fScreenWidth + ((2 * fScreenWidth) / ((float)iWaveResolution + (float)1)) * (float)iWavePart, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed + i / fWaveLength) * fWaveHeight, 0);
            vertices[i + 1] = new Vector3(-fScreenWidth + ((2 * fScreenWidth) / ((float)iWaveResolution + (float)1)) * (float)iWavePart, -fBottomPosition - (Mathf.Sin((Time.time + fBottomWaveOffset) * fWaveSpeed + i / fWaveLength)) * fWaveHeight, 0);
            iWavePart++;
        }

        iWavePart = 1;

        mesh.vertices = vertices;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // CONNECT VERTICES TO TRIANGLES -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        var tris = new int[6 + (iWaveResolution * 6)];

        int changeRect = 0;

        for (int i = 0; i < iWaveResolution + 1; i++)
        {

            if (bDrawFirstRectangle == true)
            {
                tris[i * 6] = changeRect + 1;
                tris[i * 6 + 1] = changeRect;
                tris[i * 6 + 2] = changeRect + 3;

                tris[i * 6 + 3] = changeRect;
                tris[i * 6 + 4] = changeRect + 2;
                tris[i * 6 + 5] = changeRect + 3;

                changeRect += 4;
            }


            if (bDrawFirstRectangle == false)
            {
                tris[i * 6] = changeRect;
                tris[i * 6 + 1] = changeRect + 1;
                tris[i * 6 + 2] = changeRect - 2;

                tris[i * 6 + 3] = changeRect + 1;
                tris[i * 6 + 4] = changeRect - 1;
                tris[i * 6 + 5] = changeRect - 2;
            }


            if (bDrawFirstRectangle == true)
            {
                bDrawFirstRectangle = false;
            }

            else
            {
                bDrawFirstRectangle = true;
            }

        }

        changeRect = 0;

        bDrawFirstRectangle = true;

        mesh.triangles = tris;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GIVE EACH VERTICE A NORMAL -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        var normals = new Vector3[4 + (iWaveResolution * 2)];

        for (int i = 0; i < (3 + (iWaveResolution * 2)); i++)
        {
            normals[i] = -Vector3.forward;
        }

        mesh.normals = normals;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GIVE EACH VERTICE A UV MAP (TEXTURE/ MATERIAL) -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        var uv = new Vector2[4 + (iWaveResolution * 2)];

        for (int i = 0; i <= (4 + (iWaveResolution * 2)) - 1; i++)
        {
            uv[i] = new Vector2(vertices[i].x / ((float)2 * fScreenWidth), 1 - (vertices[i].y / -fScreenHeigh));
        }

        mesh.uv = uv;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GIVE EACH VERTICE A COLOR -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = cWaveColor;
        }

        mesh.colors = colors;

    }



    void refreshMidWaveMesh()
    {

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // CREATE VERTICES AT GIVEN POINTS -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        var vertices = new Vector3[4 + (iWaveResolution * 2)];

        vertices[0] = new Vector3(-fScreenWidth, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed) * fWaveHeight, 0);       // ERSTER VERTEX LINKS OBEN 
        vertices[1] = new Vector3(-fScreenWidth, -fBottomPosition - Mathf.Sin((Time.time + fBottomWaveOffset) * fWaveSpeed) * fWaveHeight, 0);   // ZWEITER VERTEX LINKS UNTEN 

        vertices[iWaveResolution * 2 + 2] = new Vector3(fScreenWidth, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed + ((2 * iWaveResolution + 1) / fWaveLength)) * fWaveHeight, 0);       // ERSTER VERTEX RECHTS OBEN 
        vertices[iWaveResolution * 2 + 3] = new Vector3(fScreenWidth, -fBottomPosition - Mathf.Sin((Time.time + fBottomWaveOffset) * fWaveSpeed + ((2 * iWaveResolution + 2) / fWaveLength)) * fWaveHeight, 0);   // ZWEITER VERTEX RECHTS UNTEN 

        for (int i = 2; i <= iWaveResolution * 2; i += 2)
        {
            vertices[i] = new Vector3(-fScreenWidth + ((2 * fScreenWidth) / ((float)iWaveResolution + (float)1)) * (float)iWavePart, fTopPosition + Mathf.Sin((Time.time + fTopWaveOffset) * fWaveSpeed + i / fWaveLength) * fWaveHeight, 0);
            vertices[i + 1] = new Vector3(-fScreenWidth + ((2 * fScreenWidth) / ((float)iWaveResolution + (float)1)) * (float)iWavePart, -fBottomPosition - (Mathf.Sin((Time.time  + fBottomWaveOffset) * fWaveSpeed + i / fWaveLength)) * fWaveHeight, 0);
            iWavePart++;
        }

        iWavePart = 1;

        mfMidWaveMeshFilter.mesh.vertices = vertices;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GIVE EACH VERTICE A COLOR -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = cWaveColor;
        }

        mfMidWaveMeshFilter.sharedMesh.colors = colors;
    }



}
