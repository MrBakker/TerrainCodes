using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using HouseSystem;
using TreeSystem;
using GrassSystem;

public class Create : MonoBehaviour
{
    public Terrain t;
    public Terrain RiverTerrain;
    public GameObject bridgePrefab;
    //testcube
    public GameObject pallisade;
    public int numberOfTextures = 3;
    // Used for determening the pathPoints for the main roads in cities by adding or subtracting this number from the riverPoints in that city.
    public int pathWideness = 4;
    public float pathperiod = 7f;
    public float pathOffsetFactor = 4f;
    public int sidePathWideness = 2;
    public int minimumSidePathLength = 25;
    public int sidePathLengthReduction = 5;
    public float sidePathperiod = 7f;
    public float sidePathOffsetFactor = 10f;
    public int minimalSidePathDifference = 35;
    public int maximalSidePathDifference = 70;
    public int minimalSidePathRiverDifference = 5;
    public int maximalSidePathRiverDifference = 15;
    public int minimalHouseMainPathDifference = 15;
    public int maximalHouseMainPathDifference = 25;
    public int minimalHouseDifference = 18;
    public int maximalHouseDifference = 28;
    public float minHouseOffset = 4f;
    public float maxHouseOffset = 6f;
    public int houseWidth = 6;
    public int houseDepth = 8;
    public int minimalRiverPointDifference = 25;
    public int riverWideness = 5;
    public float riverperiod = 10f;
    public float riverOffsetFactor = 10f;
    public float riverLowering = 0.005f;
    public int width = 256; // Links
    public int height = 256; // Omhoog
    public int depth = 80;
    public float scale = 20;
    public float spread = 30;
    public float GrassAmount = 40f;
    public float TreeAmount = 400f;
    [HideInInspector] public int aWidth;
    [HideInInspector] public int aHeight;
    [HideInInspector] public int pathPointNumber1;
    [HideInInspector] public int pathPointNumber2;
    [HideInInspector] public int riverPointNumber1;
    [HideInInspector] public int riverPointNumber2;
    [HideInInspector] public float offsetX;
    [HideInInspector] public float offsetY;

    public TerrainData filter;
    [System.Serializable] struct cord
    {
        public int x, y;
    };

    private cord[] dx_dy_sidePathPoints;
    private cord bridgePoint1;
    private cord bridgePoint2;
    private cord p1;
    private cord p2;
    private cord first;
    private int dx;
    private int dy;
    private int max;
    private int riverGenerationAttempts = 0;
    private int pathGenerationAttempts = 0;
    private int sidePathGenerationAttempts = 0;
    int abs(int input)
    {
        if (input > 0) return input;
        else return input * -1;
    }

    HouseGenerator housegenerator = new HouseGenerator();
    TreeManager treemanager = new TreeManager();
    GrassManager grassmanager = new GrassManager();

    void Start()
    {
        housegenerator.SetUp();

        treemanager.SetTerrainSize(width, height);
        treemanager.SetTerrain(t);

        grassmanager.SetUp();
        grassmanager.SetTerrainSize(width, height);
        grassmanager.SetTerrain(t);

        aWidth = t.terrainData.alphamapWidth;
        aHeight = t.terrainData.alphamapHeight;

        bridgePoint1 = new cord { x = width + 1, y = height + 1 };
        bridgePoint2 = new cord { x = width + 1, y = height + 1 };

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        grassmanager.PrintGrass();
        treemanager.PrintTrees();
        Debug.Log("Runtime: " + Time.realtimeSinceStartup);

    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        float[,] CityHeight = new float[width, height];
        float[,] mainTerrain = new float[width, height];
        float[,] river = new float[width, height];
        float[,] path = new float[aWidth, aHeight];
        float[,] sidePath = new float[aWidth, aHeight];
        float[,] riverTerrainHeights = new float[width, height];
        mainTerrain = GenerateHeights(0.80f, 4f);

        // Genereer extra basisfilters voor het terrein (telt voor 1/x mee)
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);

        float[,] filter = new float[width, height];
        filter = GenerateHeights(0.15f, scale);

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);

        float[,] filter2 = new float[width, height];
        filter2 = GenerateHeights(0.05f, scale);

        int x, y;
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                mainTerrain[x, y] += filter[x, y] + filter2[x, y];
            }
        }

        //Genereer stad/dorp in boolean map (0 = terrein; 1 = stad/dorp)
        float[,] cityTerrain = new float[width, height];
        Debug.Log("City: 127, 127");
        GetCity(127, 127, 8000, ref cityTerrain, ref mainTerrain, ref CityHeight, ref river, ref path, ref sidePath);
        // Debug.Log("City: 180, 100");
        // GetCity(180, 100, 1600, ref cityTerrain, ref mainTerrain, ref CityHeight, ref river, ref path, ref sidePath);
        // Debug.Log("City: 50, 50");
        // GetCity(50, 50, 2600, ref cityTerrain, ref mainTerrain, ref CityHeight, ref river, ref path, ref sidePath);
        // Debug.Log("City: 100, 200");
        // GetCity(100, 200, 2600, ref cityTerrain, ref mainTerrain, ref CityHeight, ref river, ref path, ref sidePath);


        // GetCity(128, 128, 2600, ref cityTerrain, ref mainTerrain, ref CityHeight, ref river, ref path, ref sidePath);


        //print(ref river);

        // Genereer olievlek (overvloeien platte vlak stad met landschap)


        // Schrijf de gekregen gegevens om naar een factor (0 <= f <= 1)
        // Formule: 0.5 * sin(x - 0.5π) + 0.5
        float[] factorValues = new float[(int)spread + 1];
        for (int i = 0; i <= (int)spread; i++)
        {
            factorValues[i] = 0.5f * Mathf.Sin(i / spread * Mathf.PI - 0.5f * Mathf.PI) + 0.5f;
        }


        // Schrijf het cityterrain over naar een filter
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                if (cityTerrain[x, y] > 1) cityTerrain[x, y] = factorValues[(int)cityTerrain[x, y]];
                else if (cityTerrain[x, y] == 1) cityTerrain[x, y] = 0;
                else cityTerrain[x, y] = 1;
            }
        }

        Debug.Log("Generating River & Paths");
        float[,,] textureMap = new float[aWidth, aHeight, numberOfTextures];
        bool[,] holeMap = new bool[width, height];
        // Combine filters


        for (x = 0; x < aWidth; x++)
        {
            for (y = 0; y < aHeight; y++)
            {
                //painting paths;
                if (path[x, y] != 0 || sidePath[x, y] != 0)
                {
                    textureMap[x, y, 0] = 0f;
                    textureMap[x, y, 1] = 0f;
                    textureMap[x, y, 2] = 1f;
                }
            }
        }


        Debug.Log("aWidth = " + aWidth + "; aHeight = " + aHeight);
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                holeMap[x, y] = true;
                if (cityTerrain[x, y] != 1)
                {
                    mainTerrain[x, y] *= cityTerrain[x, y];
                    mainTerrain[x, y] += (1f - cityTerrain[x, y]) * CityHeight[x, y];

                    // river[x,y] is float between [1, 5]
                    if (river[x, y] != 0 && river[x, y] < 6)
                    {

                        // generate terrain with water
                        holeMap[x, y] = false;
                        riverTerrainHeights[x, y] = mainTerrain[x, y] - riverLowering;

                        // lower terrain at rivers
                        mainTerrain[x, y] -= 0.075f * Mathf.Cos(Mathf.PI * river[x, y] / 10f);

                        // paint rivertexture
                        if (numberOfTextures != 0)
                        {
                            for (int i = 0; i < aWidth / width; i++)
                            {
                                for (int j = 0; j < aHeight / height; j++)
                                {
                                    textureMap[x * aWidth / width + i, y * aHeight / height + j, 0] = 1f - (10f - (float)river[x, y]) / 9f;
                                    textureMap[x * aWidth / width + i, y * aHeight / height + j, 1] = (10f - (float)river[x, y]) / 9f;
                                    textureMap[x * aWidth / width + i, y * aHeight / height + j, 2] = 0f;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (river[x, y] != 0)
                        {
                            holeMap[x, y] = false;
                        }

                        if (numberOfTextures != 0)
                        {

                            for (int i = 0; i < aWidth / width; i++)
                            {
                                for (int j = 0; j < aHeight / height; j++)
                                {
                                    textureMap[x * aWidth / width + i, y * aHeight / height + j, 0] = 1f;
                                    textureMap[x * aWidth / width + i, y * aHeight / height + j, 1] = 0f;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (numberOfTextures != 0)
                    {
                        for (int i = 0; i < aWidth / width; i++)
                        {
                            for (int j = 0; j < aHeight / height; j++)
                            {
                                textureMap[x * aWidth / width + i, y * aHeight / height + j, 0] = 1f;
                                textureMap[x * aWidth / width + i, y * aHeight / height + j, 1] = 0f;
                            }
                        }
                    }
                }
            }
        }




        //printBool(ref holeMap);
        RiverTerrain.terrainData.SetHoles(0, 0, holeMap);

        RiverTerrain.terrainData.SetHeights(0, 0, riverTerrainHeights);

        t.terrainData.SetAlphamaps(0, 0, textureMap);

        terrainData.SetHeights(0, 0, mainTerrain);

        Debug.Log(mainTerrain[100, 100] * depth);

        return terrainData;
    }

    /// used to debug to another file
    void print(ref float[,] input)
    {
        string path = "Assets/Resources/test.txt";
        string outputfunction = "";
        int x, y;
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                outputfunction += (input[x, y]).ToString();
            }
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(outputfunction);
            writer.Close();
            outputfunction = "";
        }
    }

    void printBool(ref bool[,] input)
    {
        string path = "Assets/Resources/test.txt";
        string outputfunction = "";
        int x, y;
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                if (input[x, y] == false) outputfunction += "0";
                else outputfunction += "1";
                // outputfunction += (input[x, y]).ToString();
            }
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(outputfunction);
            writer.Close();
            outputfunction = "";
        }
    }

    void GenerateOil(int xmin, int ymin, int xmax, int ymax, ref float[,] cityTerrain, ref float[,] CityHeights, float AverageHeight)
    {
        int x, y;
        for (x = xmin; x <= xmax; x++)
        {
            float PreviousValue = 0;
            for (y = 0; y < height; y++)
            {
                if (PreviousValue > 0)
                {
                    if (cityTerrain[x, y] == 0 && PreviousValue + 1 < spread || cityTerrain[x, y] > PreviousValue + 1)
                    {
                        cityTerrain[x, y] = PreviousValue + 1;
                        CityHeights[x, y] = AverageHeight;
                    }
                }
                PreviousValue = cityTerrain[x, y];
            }

            PreviousValue = 0;
            for (y = y - 1; y >= 0; y--)
            {
                if (PreviousValue > 0)
                {
                    if (cityTerrain[x, y] == 0 && PreviousValue + 1 < spread || cityTerrain[x, y] > PreviousValue + 1)
                    {
                        cityTerrain[x, y] = PreviousValue + 1;
                        CityHeights[x, y] = AverageHeight;
                    }
                }
                PreviousValue = cityTerrain[x, y];
            }
        }

        for (y = ymin; y < ymax; y++)
        {
            float PreviousValue = 0;
            for (x = 0; x < width; x++)
            {
                if (PreviousValue > 0)
                {
                    if (cityTerrain[x, y] == 0 && PreviousValue + 1 < spread || cityTerrain[x, y] > PreviousValue + 1)
                    {
                        cityTerrain[x, y] = PreviousValue + 1;
                        CityHeights[x, y] = AverageHeight;
                    }
                }
                PreviousValue = cityTerrain[0 + x, 0 + y];
            }

            PreviousValue = 0;
            for (x = x - 1; x >= 0; x--)
            {
                if (PreviousValue > 0)
                {
                    if (cityTerrain[x, y] == 0 && PreviousValue + 1 < spread || cityTerrain[x, y] > PreviousValue + 1)
                    {
                        cityTerrain[x, y] = PreviousValue + 1;
                        CityHeights[x, y] = AverageHeight;
                    }
                }
                PreviousValue = cityTerrain[0 + x, 0 + y];
            }
        }
    }

    void GenerateOil_river(ref float[,] river, int spread)
    {
        int x, y;
        for (x = 0; x < width; x++)
        {
            float PreviousValue = 0;
            for (y = 0; y < height; y++)
            {
                if (PreviousValue > 0)
                {
                    if (river[x, y] == 0 && PreviousValue + 1 < spread || river[x, y] > PreviousValue + 1)
                    {
                        river[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = river[x, y];
            }

            PreviousValue = 0;
            for (y = y - 1; y >= 0; y--)
            {
                if (PreviousValue > 0)
                {
                    if (river[x, y] == 0 && PreviousValue + 1 < spread || river[x, y] > PreviousValue + 1)
                    {
                        river[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = river[x, y];
            }
        }

        for (y = 0; y < height; y++)
        {
            float PreviousValue = 0;
            for (x = 0; x < width; x++)
            {
                if (PreviousValue > 0)
                {
                    if (river[x, y] == 0 && PreviousValue + 1 < spread || river[x, y] > PreviousValue + 1)
                    {
                        river[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = river[x, y];
            }

            PreviousValue = 0;
            for (x = x - 1; x >= 0; x--)
            {
                if (PreviousValue > 0)
                {
                    if (river[x, y] == 0 && PreviousValue + 1 < spread || river[x, y] > PreviousValue + 1)
                    {
                        river[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = river[x, y];
            }
        }
    }

    void GenerateOil_path(ref float[,] path, int spread)
    {
        int x, y;
        for (x = 0; x < aWidth; x++)
        {
            float PreviousValue = 0;
            for (y = 0; y < aHeight; y++)
            {
                if (PreviousValue > 0)
                {
                    if (path[x, y] == 0 && PreviousValue + 1 < spread || path[x, y] > PreviousValue + 1)
                    {
                        path[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = path[x, y];
            }

            PreviousValue = 0;
            for (y = y - 1; y >= 0; y--)
            {
                if (PreviousValue > 0)
                {
                    if (path[x, y] == 0 && PreviousValue + 1 < spread || path[x, y] > PreviousValue + 1)
                    {
                        path[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = path[x, y];
            }
        }

        for (y = 0; y < aHeight; y++)
        {
            float PreviousValue = 0;
            for (x = 0; x < aWidth; x++)
            {
                if (PreviousValue > 0)
                {
                    if (path[x, y] == 0 && PreviousValue + 1 < spread || path[x, y] > PreviousValue + 1)
                    {
                        path[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = path[x, y];
            }

            PreviousValue = 0;
            for (x = x - 1; x >= 0; x--)
            {
                if (PreviousValue > 0)
                {
                    if (path[x, y] == 0 && PreviousValue + 1 < spread || path[x, y] > PreviousValue + 1)
                    {
                        path[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = path[x, y];
            }
        }

    }

    void GenerateOil_sidePath(ref float[,] sidePath, int spread)
    {
        int x, y;
        for (x = 0; x < aWidth; x++)
        {
            float PreviousValue = 0;
            for (y = 0; y < aHeight; y++)
            {
                if (PreviousValue > 0)
                {
                    if (sidePath[x, y] == 0 && PreviousValue + 1 < spread || sidePath[x, y] > PreviousValue + 1)
                    {
                        sidePath[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = sidePath[x, y];
            }

            PreviousValue = 0;
            for (y = y - 1; y >= 0; y--)
            {
                if (PreviousValue > 0)
                {
                    if (sidePath[x, y] == 0 && PreviousValue + 1 < spread || sidePath[x, y] > PreviousValue + 1)
                    {
                        sidePath[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = sidePath[x, y];
            }
        }

        for (y = 0; y < aHeight; y++)
        {
            float PreviousValue = 0;
            for (x = 0; x < aWidth; x++)
            {
                if (PreviousValue > 0)
                {
                    if (sidePath[x, y] == 0 && PreviousValue + 1 < spread || sidePath[x, y] > PreviousValue + 1)
                    {
                        sidePath[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = sidePath[x, y];
            }

            PreviousValue = 0;
            for (x = x - 1; x >= 0; x--)
            {
                if (PreviousValue > 0)
                {
                    if (sidePath[x, y] == 0 && PreviousValue + 1 < spread || sidePath[x, y] > PreviousValue + 1)
                    {
                        sidePath[x, y] = PreviousValue + 1;
                    }
                }
                PreviousValue = sidePath[x, y];
            }
        }

    }


    void GetCity(int centerx, int centery, int Inhoud, ref float[,] cityTerrain, ref float[,] mainTerrain, ref float[,] CityHeight, ref float[,] river, ref float[,] path, ref float[,] sidePath)
    {
        float AverageLength = (float)Mathf.Sqrt(Inhoud / Mathf.PI);
        float DiffLength = 0f;
        float AverageHeight = 0f;
        if (AverageLength > 20f) DiffLength = (AverageLength - 10f) * 0.35f;
        else if (AverageLength > 10f) DiffLength = (AverageLength - 5f) * (3.5f / 15f);
        else DiffLength = 0f;
        cord[] cityPerimeter = new cord[60];
        float firstlength = 0f;

        if (centerx + AverageLength + DiffLength < height && centerx - AverageLength - DiffLength > 0 && centery + AverageLength + DiffLength > width && centery - AverageLength - DiffLength > 0) return;

        for (int i = 0; i <= 60; i++)
        { // Bereken 60 verschillende coordinaten van de randpunten van een stad
            float length = 0f;
            int index = 0;
            if (i < 60)
            {
                // Bereken x, y positie randpunten stad
                index = i;
                length = AverageLength - DiffLength + CalculateHeight(0, i, 25f) * DiffLength * 2f;
                if (i == 0) firstlength = length;
                else if (i > 50) length += (firstlength - length) / 10 * (i - 50);
                cityPerimeter[i] = new cord { x = centerx + (int)(Mathf.Cos(2f * Mathf.PI / 60f * (float)i) * length), y = centery + (int)(Mathf.Sin(2f * Mathf.PI / 60f * (float)i) * length) };
                AverageHeight += mainTerrain[cityPerimeter[i].x, cityPerimeter[i].y];
            }
            else index = 0;
        }
        AverageHeight /= 60;
        List<int> borderx = new List<int>();
        List<int> bordery = new List<int>();

        for (int i = 1, index; i <= 60; i++)
        {
            if (i < 60) index = i;
            else index = 0;
            max = 0;
            dx = cityPerimeter[i - 1].x - cityPerimeter[index].x;
            dy = cityPerimeter[i - 1].y - cityPerimeter[index].y;
            if (abs(dx) < abs(dy)) max = abs(dy);
            else max = abs(dx);
            for (int n = 0, outx, outy; n <= max + 1; n++)
            {
                outx = CheckX(cityPerimeter[index].x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                outy = CheckY(cityPerimeter[index].y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));
                CityHeight[outx, outy] = AverageHeight;
                cityTerrain[outx, outy] = 1;
                borderx.Add(outx);
                bordery.Add(outy);
            }
        }

        // Genereer boolean map voor plaatsbepaling stad
        int[] diffX = new int[4] { 0, 1, 0, -1 };
        int[] diffY = new int[4] { 1, 0, -1, 0 };
        float[,] test = new float[width, height];
        test = cityTerrain;
        void Rec(int x, int y, ref float[,] CityHeight)
        {
            if (x <= 0 || y <= 0 || x >= width || y >= height) return;
            for (int i = 0; i < 4; i++)
            {
                if (test[x + diffX[i], y + diffY[i]] == 0)
                {
                    CityHeight[x + diffX[i], y + diffY[i]] = AverageHeight;
                    test[x + diffX[i], y + diffY[i]] = 1;
                    Rec(x + diffX[i], y + diffY[i], ref CityHeight);
                }
            }
        }

        Rec(centerx, centery, ref CityHeight); // Recursieve functie voor opvulling stad terrein.
        cityTerrain = test;

        GenerateOil(CheckX((int)(centerx - AverageLength - DiffLength - spread - 1)), CheckY((int)(centery - AverageLength - DiffLength - spread - 1)), CheckX((int)(centerx + AverageLength + DiffLength + spread + 1)), CheckY((int)(centery + AverageLength + DiffLength + spread + 1)), ref cityTerrain, ref CityHeight, AverageHeight);
        bool[,] SpaceMap = new bool[width, height];
        riverGenerationAttempts = 0;
        GenerateRiver(cityPerimeter, ref cityTerrain, ref river, ref mainTerrain, ref SpaceMap);
        
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                if (cityTerrain[x, y] == 1 && river[x, y] == 0 && noBorder(x, y) == true) SpaceMap[x, y] = true;
                else SpaceMap[x, y] = false;
            }
        }
        
        
        GenerateCityPaths(cityPerimeter, ref cityTerrain, ref path, ref river, ref CityHeight, ref mainTerrain, ref sidePath, ref AverageHeight, ref SpaceMap);
        GenerateOil_path(ref path, pathWideness * aHeight / height);
        GenerateOil_sidePath(ref sidePath, sidePathWideness * aHeight / height);
        
        for (int x = 0; x < aWidth; x++){
            for (int y = 0; y < aHeight; y++){
                if (path[x, y] == 1 || sidePath[x, y] == 1) SpaceMap[(int)((float)x * ((float)width / (float)aWidth)), (int)((float)y * ((float)height / (float)aHeight))] = false;
            }
        }

        bool noBorder(int x, int y){
            for (int i = 0; i < borderx.Count; i++){
                if (borderx[i] == x && bordery[i] == y) return false;
            }
            return true;
        }

        for (int x = 0; x < aWidth; x++){
            for (int y = 0; y < aHeight; y++){
                if (SpaceMap[(int)((float)x * ((float)width / (float)aWidth)), (int)((float)y * ((float)height / (float)aHeight))] == true && path[x, y] == 0 && sidePath[x, y] == 0){
                    if ((int)Random.Range(0f, GrassAmount) == 0) {
                        grassmanager.AddGrass(new Vector3((float)x * ((float)width / (float)aWidth), 0f, (float)y * ((float)height / (float)aHeight)));
                    }
                    if ((int)Random.Range(0f, TreeAmount) == 0){
                        treemanager.AddTree(new Vector3((float)y * ((float)width / (float)aWidth), AverageHeight, (float)x * ((float)height / (float)aHeight)));
                        Debug.Log(AverageHeight);
                    }
                }
            }
        }

        GameObject best = new GameObject("All Borders"), border;
        for (int i = 0; i < borderx.Count; i++){
            if (river[borderx[i], bordery[i]] == 0 && path[(int)((float)borderx[i] * ((float)aWidth / (float)width)), (int)((float)bordery[i] * ((float)aHeight / (float)height))] == 0){
                border = Instantiate(pallisade, new Vector3(bordery[i], AverageHeight * depth, borderx[i]), Quaternion.identity) as GameObject;
                border.transform.SetParent(best.transform);
                border.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                border.transform.LookAt(new Vector3(centery, AverageHeight * depth, centerx));
            }
            
        }
    }

    int CheckX(int input)
    {
        if (input < 0) return 0;
        if (input >= width) return width - 1;
        return input;
    }

    int CheckY(int input)
    {
        if (input < 0) return 0;
        if (input >= height) return height - 1;
        return input;
    }

    int CheckAX(int input)
    {
        if (input < 0) return 0;
        if (input >= aWidth) return aWidth - 1;
        return input;
    }

    int CheckAY(int input)
    {
        if (input < 0) return 0;
        if (input >= aHeight) return aHeight - 1;
        return input;
    }

    float[,] GenerateHeights(float factor, float scale)
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y, scale) * factor;
            }
        }

        return heights;
    }
    float CalculateHeight(int x, int y, float scale)
    {
        float xCoord, yCoord;
        xCoord = (float)x / width * scale + offsetX;
        yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

        void GenerateRiver(cord[] city, ref float[,] cityTerrain, ref float[,] river, ref float[,] mainTerrain, ref bool[,] SpaceMap)
    {

        // Debug.Log("You came");
        // Debug.Log("Generation Attempt: " + riverGenerationAttempts);
        if ((int)riverGenerationAttempts >= 20)
        {
            //Debug.Log("Whoops, error");
            return;
        }
        else
        {
            float[,] output = new float[width, height];

            //sixty boundary points of the city
            // a river will be generated between these two points
            riverPointNumber1 = (int)Random.Range(0, 59);
            riverPointNumber2 = (riverPointNumber1 + (int)Random.Range(minimalRiverPointDifference, 59 - minimalRiverPointDifference)) % 60; //may not overflow the array of boundary points

            //Debug.Log("Riverpoints: " + riverPointNumber1 + ", " + riverPointNumber2);

            cord RiverPoint1 = city[riverPointNumber1];
            cord RiverPoint2 = city[riverPointNumber2];

            //draw line between city points for referance
            dx = RiverPoint2.x - RiverPoint1.x;
            dy = RiverPoint2.y - RiverPoint1.y;

            //rico from the normal, used by riveroffset
            int dx_offset = dy;
            int dy_offset = -dx;

            max = 0;
            // calculate the maximum delta
            if (abs(dx) < abs(dy)) max = abs(dy);
            else max = abs(dx);

            // generate an empty array of cords, the size of the total number of RiverPoints
            cord[] RiverPoints = new cord[max + 1];

            //generate points, which the river lies between
            for (int n = 0; n <= max; n++)
            {

                //generaterate the coordinates of the points in between the start- and end-point, which makes a straight line
                int RiverPointx = CheckX(RiverPoint1.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                int RiverPointy = CheckY(RiverPoint1.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));

                // Calculate the offset differrence, this is the difference between the delta height of the start and end point and
                // the perlin noise height of the current point. This is to make sure the river starts and ends where the start and end
                // points are and to make it meander more.
                float riverOffsetDifference = ((CalculateHeight(0, max, riverperiod) - CalculateHeight(0, 0, riverperiod)) / max) * n + CalculateHeight(0, 0, riverperiod);
                //calculate the offset for each point.
                float riveroffset = riverOffsetFactor * (CalculateHeight(0, n, riverperiod) - riverOffsetDifference);

                // calculate the offset for each point, this is used to calculate the xOffset and yOffset
                float RiverPointOffsetFactor = riveroffset / Mathf.Sqrt(Mathf.Pow((int)dx_offset, 2f) + Mathf.Pow((int)dy_offset, 2f));

                //calculate the coordinates of the new RiverPoints
                RiverPointx += (int)Mathf.Round((int)dx_offset * (float)RiverPointOffsetFactor);
                RiverPointy += (int)Mathf.Round((int)dy_offset * (float)RiverPointOffsetFactor);

                //push them to the list of RiverPoints
                RiverPoints[n] = new cord
                {
                    x = RiverPointx,
                    y = RiverPointy
                };

                // Debug.Log("x: " + RiverPoints[n].x);
                // Debug.Log("y: " + RiverPoints[n].y);

            }

            for (int i = 0; i < RiverPoints.Length - 1; i++)
            {
                // Debug.Log(i + " " + RiverPoints.Length);
                dx = RiverPoints[i + 1].x - RiverPoints[i].x;
                dy = RiverPoints[i + 1].y - RiverPoints[i].y;

                //the distance between 2 points over 1 axis
                int distance = 0;
                // calculate the maximum delta
                if (abs(dx) < abs(dy)) distance = abs(dy);
                else distance = abs(dx);

                // generate all riverpoints in cities
                for (int n = 0; n <= distance; n++)
                {

                    int RiverPointx = CheckX(RiverPoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n)));
                    int RiverPointy = CheckY(RiverPoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n)));

                    // check if river stays inside city bounds
                    if (cityTerrain[RiverPointx, RiverPointy] != 1)
                    {

                        riverGenerationAttempts++;

                        GenerateRiver(city, ref cityTerrain, ref river, ref mainTerrain, ref SpaceMap);
                        return;
                    }
                    output[RiverPointx, RiverPointy] = 1;
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // output filter is copied to river filter
                    if (output[x, y] == 1) river[x, y] = output[x, y];
                }
            }
        }

        GenerateOil_river(ref river, riverWideness);
    }

    void GenerateCityPaths(cord[] city, ref float[,] cityTerrain, ref float[,] path, ref float[,] river, ref float[,] CityHeight, ref float[,] mainTerrain, ref float[,] sidePath, ref float AverageHeight, ref bool[,] SpaceMap)
    {

        if ((int)pathGenerationAttempts >= 20)
        {
            //Debug.Log("Whoops, error");
            return;
        }
        else
        {




            // rekening houden met plaats voor huizen.
            float[,] output = new float[aWidth, aHeight];
            //sixty boundary points of the city
            // a path will be generated between these two points

            // -- Generate starting and end positions.

            int averageRiverPoint = modulo(((int)Mathf.Round(riverPointNumber1 + riverPointNumber2)) / 2, 60);

            //difference between the average riverpoint and pathpoints to make sure paths and rivers dont overlap completely.
            int PathRiverDifference = Mathf.Abs(averageRiverPoint - riverPointNumber1);

            pathPointNumber1 = modulo(averageRiverPoint + (Random.Range(Mathf.RoundToInt(-PathRiverDifference / 2), Mathf.RoundToInt(PathRiverDifference / 2))), 60);

            pathPointNumber2 = modulo(averageRiverPoint + 30 + (Random.Range(Mathf.RoundToInt(-PathRiverDifference / 2), Mathf.RoundToInt(PathRiverDifference / 2))), 60);

            //Debug.Log("a1");
            //Debug.Log("Pathpoints: " + pathPointNumber1 + ", " + pathPointNumber2);


            cord pathPoint1 = city[pathPointNumber1];
            cord pathPoint2 = city[pathPointNumber2];

            //multiply by the difference between resolutions of the heightmap and the alphamap to achieve further detail.
            pathPoint1.x *= aWidth / width;
            pathPoint1.y *= aHeight / height;
            pathPoint2.x *= aWidth / width;
            pathPoint2.y *= aHeight / height;

            //Debug.Log("pathpoints: " + pathPointNumber1 + ", " + pathPointNumber2);

            //draw line between city points for referance
            dx = pathPoint2.x - pathPoint1.x;
            dy = pathPoint2.y - pathPoint1.y;

            //rico from the normal, used by pathoffset
            int dx_offset = dy;
            int dy_offset = -dx;

            max = 0;
            // calculate the maximum delta
            if (abs(dx) < abs(dy)) max = abs(dy);
            else max = abs(dx);

            // generate an empty array of cords, the size of the total number of pathPoints
            cord[] pathPoints = new cord[max + 1];

            // this is the straight line, which the path is based on
            cord[] pathLine = new cord[max + 1];

            //generate points, which the path lies between
            for (int n = 0; n <= max; n++)
            {

                //generaterate the coordinates of the points in between the start- and end-point, which makes a straight line
                int pathPointx = CheckAX(pathPoint1.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                int pathPointy = CheckAY(pathPoint1.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));

                pathLine[n] = new cord
                {
                    x = pathPointx,
                    y = pathPointy
                };

                // Calculate the offset differrence, this is the difference between the delta height of the start and end point and
                // the perlin noise height of the current point. This is to make sure the path starts and ends where the start and end
                // points are and to make it meander more.
                float pathOffsetDifference = ((CalculateHeight(0, max, pathperiod) - CalculateHeight(0, 0, pathperiod)) / max) * n + CalculateHeight(0, 0, pathperiod);
                //calculate the offset for each point.
                float pathoffset = pathOffsetFactor * (CalculateHeight(0, n, pathperiod) - pathOffsetDifference);

                // calculate the offsetfactor for each point, this is used to calculate the xOffset and yOffset
                float pathPointOffsetFactor = pathoffset / Mathf.Sqrt(Mathf.Pow((int)dx_offset, 2f) + Mathf.Pow((int)dy_offset, 2f));

                //calculate the coordinates of the new pathPoints
                pathPointx += (int)Mathf.Round((int)dx_offset * (float)pathPointOffsetFactor);
                pathPointy += (int)Mathf.Round((int)dy_offset * (float)pathPointOffsetFactor);


                //push them to the list of pathPoints
                pathPoints[n] = new cord
                {
                    x = pathPointx,
                    y = pathPointy
                };

                // Debug.Log("x: " + pathPoints[n].x);
                // Debug.Log("y: " + pathPoints[n].y);

            }

            //flag is used to determine start and end points of a bridge over the river
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;


            float bridgePoint1x = width + 1;
            float bridgePoint1y = height + 1;
            float bridgePoint2x = width + 1;
            float bridgePoint2y = height + 1;

            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                // Debug.Log(i + " " + pathPoints.Length);
                dx = pathPoints[i + 1].x - pathPoints[i].x;
                dy = pathPoints[i + 1].y - pathPoints[i].y;

                //the distance between 2 points over 1 axis
                int distance = 0;
                // calculate the maximum delta
                if (abs(dx) < abs(dy)) distance = abs(dy);
                else distance = abs(dx);

                // generate all pathpoints in cities
                for (int n = 0; n <= distance; n++)
                {

                    int pathPointx = CheckAX(pathPoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n)));
                    int pathPointy = CheckAY(pathPoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n)));

                    // check if river stays inside city bounds
                    if (cityTerrain[(int)Mathf.Floor(pathPointx / (aWidth / width)), (int)Mathf.Floor(pathPointy / (aHeight / height))] != 1)
                    {

                        pathGenerationAttempts++;

                        flag1 = false;
                        flag2 = false;
                        flag3 = false;
                        flag4 = false;

                        GenerateCityPaths(city, ref cityTerrain, ref path, ref river, ref CityHeight, ref mainTerrain, ref sidePath, ref AverageHeight, ref SpaceMap);
                        return;
                    }

                    output[pathPointx, pathPointy] = 1;


                    //Instantiate(cube, new Vector3((int)Mathf.Floor(pathPointy/(aHeight/height)), 40f, (int)Mathf.Floor(pathPointx/(aWidth/width))), Quaternion.identity);


                    if (river[(int)Mathf.Floor(pathPointx / (aWidth / width)), (int)Mathf.Floor(pathPointy / (aHeight / height))] > 0)
                    {
                        flag1 = true;
                        //Debug.Log("Flag");
                    }
                    else
                    {
                        flag1 = false;
                        //Debug.Log("NoFlag");
                    }
                    if (flag1 == true && flag2 == false && flag3 == false && flag4 == false)
                    {
                        //Debug.Log("Flagged");
                        bridgePoint1x = pathPointx / (aWidth / width);
                        bridgePoint1y = pathPointy / (aHeight / height);
                        bridgePoint1 = new cord { x = (int)Mathf.Floor(pathPointx / (aWidth / width)), y = (int)Mathf.Floor(pathPointy / (aHeight / height)) };
                    }
                    if (flag1 == false && flag2 == true && flag3 == true && flag4 == true)
                    {
                        //Debug.Log("Unflagged");
                        bridgePoint2x = pathPointx / (aWidth / width);
                        bridgePoint2y = pathPointy / (aHeight / height);
                        bridgePoint2 = new cord { x = (int)Mathf.Floor(pathPointx / (aWidth / width)), y = (int)Mathf.Floor(pathPointy / (aHeight / height)) };
                    }

                    flag4 = flag3;
                    flag3 = flag2;
                    flag2 = flag1;
                }
            }
            for (int x = 0; x < aWidth; x++)
            {
                for (int y = 0; y < aHeight; y++)
                {
                    // output filter is copied to path filter
                    if (output[x, y] == 1) path[x, y] = output[x, y];
                }
            }

            float bridgeHeight = CityHeight[bridgePoint1.x, bridgePoint1.y] * depth - 0.12f; // -3.6f

            // instantiate bridge
            if (bridgePoint1.x != width + 1 && bridgePoint2.x != width + 1)
            {

                //Debug.Log("generating bidge");
                dx = bridgePoint2.x - bridgePoint1.x;
                dy = bridgePoint2.y - bridgePoint1.y;
                float r = Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));

                //float bridgeRotation = (((Mathf.Asin(dy/r)) / Mathf.PI) * 180f) + 90f;

                //Quaternion rotation = Quaternion.Euler(0f, bridgeRotation, 0f);

                if (bridgePoint1x != width + 1 && bridgePoint2x != width + 1 && bridgePoint1y != height + 1 && bridgePoint2y != height + 1)
                {

                    Vector3 bridgePosition = new Vector3((bridgePoint1y + bridgePoint2y) / 2, bridgeHeight, (bridgePoint1x + bridgePoint2x) / 2);
                    // Vector3 direction = new Vector3(bridgePoint1.x - bridgePosition.x, bridgeHeight, bridgePoint1.y - bridgePosition.z);
                    // Quaternion rotation = Quaternion.LookRotation(direction);

                    //Instantiate(cube, new Vector3(bridgePoint1y, bridgeHeight, bridgePoint1x), Quaternion.identity);
                    //Instantiate(cube, new Vector3(bridgePoint2y, bridgeHeight, bridgePoint2x), Quaternion.identity);

                    //Debug.Log("R = " + r);
                    var bridge = Instantiate(bridgePrefab, transform.position, transform.rotation);
                    bridge.transform.position = bridgePosition;
                    bridge.transform.LookAt(new Vector3(bridgePoint1.y, bridgeHeight, bridgePoint1.x));

                    // pathwideness * 2 - 3 => min = 1

                    // 4 => 0.3

                    bridge.transform.localScale = new Vector3(1f - ((5f - pathWideness) * 0.4f), 0.3f, r / 55f);


                    //transform.rotation = Quaternion.Euler(new Vector3(0f, bridgeRotation, 0f));

                    //Instantiate(cube, bridgePosition, Quaternion.identity);
                }

                //Instantiate(bridgePrefab, bridgePosition, rotation);
                //Debug.Log("Generated Bridge");
                //Debug.Log("BridgeHeight: " + bridgeHeight);

            }

            generateCitySidePaths(pathLine, pathPoints, ref city, ref cityTerrain, ref sidePath, ref river, ref AverageHeight, ref SpaceMap, ref path);








            // Debug.Log("a1");
            // Debug.Log("Pathpoints: " + pathPointNumber1 + ", " + pathPointNumber2);


            // cord pathPoint1 = city[pathPointNumber1];
            // cord pathPoint2 = city[pathPointNumber2];

            // //multiply by the difference between resolutions of the heightmap and the alphamap to achieve further detail.
            // pathPoint1.x *= aWidth/width;
            // pathPoint1.y *= aHeight/height;
            // pathPoint2.x *= aWidth/width;
            // pathPoint2.y *= aHeight/height;

            //     //draw line between city points for referance
            //     dx = pathPoint2.x - pathPoint1.x;
            //     dy = pathPoint2.y - pathPoint1.y;

            //     //rico from the normal, used by pathoffset
            //     int dx_offset = dy;
            //     int dy_offset = -dx;

            //     max = 0;
            //     // calculate the maximum delta
            //     if (abs(dx) < abs(dy)) max = abs(dy);
            //     else max = abs(dx);

            //     // generate an empty array of cords, the size of the total number of pathPoints
            //     cord [] pathPoints = new cord[max+1];

            //     //generate points, which the path lies between
            //     for (int n = 0; n <= max; n++){

            //         //generaterate the coordinates of the points in between the start- and end-point, which makes a straight line
            //         int pathPointx = CheckX(pathPoint1.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
            //         int pathPointy = CheckY(pathPoint1.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));

            //         // Calculate the offset differrence, this is the difference between the delta height of the start and end point and
            //         // the perlin noise height of the current point. This is to make sure the path starts and ends where the start and end
            //         // points are and to make it meander more.
            //         float pathOffsetDifference = ((CalculateHeight(0,max,pathperiod)-CalculateHeight(0,0,pathperiod))/max)*n + CalculateHeight(0,0,pathperiod);
            //         //calculate the offset for each point.
            //         float pathOffset = pathOffsetFactor * (CalculateHeight(0, n, pathperiod)-pathOffsetDifference);

            //         // calculate the offset for each point, this is used to calculate the xOffset and yOffset
            //         float pathPointOffsetFactor = pathOffset/Mathf.Sqrt(Mathf.Pow((int)dx_offset,2f)+Mathf.Pow((int)dy_offset,2f));

            //         //calculate the coordinates of the new pathPoints
            //         pathPointx += (int)Mathf.Round((int)dx_offset * (float)pathPointOffsetFactor);
            //         pathPointy += (int)Mathf.Round((int)dy_offset * (float)pathPointOffsetFactor);

            //         //push them to the list of pathPoints
            //         pathPoints[n] = new cord{
            //             x = pathPointx,
            //             y = pathPointy
            //         };

            //         // Debug.Log("x: " + pathPoints[n].x);
            //         // Debug.Log("y: " + pathPoints[n].y);

            //     }

            //     for (int i = 0; i < pathPoints.Length - 1; i++)
            //     {

            //         // Debug.Log(i + " " + pathPoints.Length);
            //         dx = pathPoints[i+1].x - pathPoints[i].x;
            //         dy = pathPoints[i+1].y - pathPoints[i].y;



            //         //the distance between 2 points over 1 axis
            //         int distance = 0;
            //         // calculate the maximum delta
            //         if (abs(dx) < abs(dy)) distance = abs(dy);
            //         else distance = abs(dx);

            //         // generate all pathpoints in cities
            //         for (int n = 0; n <= distance; n++){

            //             int pathPointx = CheckX(pathPoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n)));
            //             int pathPointy = CheckY(pathPoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n)));

            //             output [pathPointx,pathPointy] = 1;
            //         }

            //     }
            //     for (int x = 0; x < width; x++){
            //         for (int y = 0; y < height; y++){
            //             // output filter is copied to path filter
            //             if (output[x, y] == 1) path[x, y] = output[x, y]; 
            //         }
            //     }
        }
    }



    void generateCitySidePaths(cord[] line, cord[] points, ref cord[] city, ref float[,] cityTerrain, ref float[,] sidePath, ref float[,] river, ref float AverageHeight, ref bool[,] SpaceMap, ref float[,] path)
    {

        float[,] output = new float[aWidth, aHeight];

        cord startPoint = new cord { x = 0, y = 0 };
        cord endPoint = new cord { x = 0, y = 0 };

        cord point1 = line[0];
        cord point2 = line[line.Length - 1];

        // line of main path
        int dx_line = point2.x - point1.x;
        int dy_line = point2.y - point1.y;

        // line of normal of main path
        int dx_normal = dy_line;
        int dy_normal = -dx_line;

        bool flag1 = false;
        bool flag2 = false;

        int startRiver = -1;
        int endRiver = -1;

        for (int i = 0; i < line.Length; i++)
        {

            if (river[(int)Mathf.Floor(points[i].x / (aWidth / width)), (int)Mathf.Floor(points[i].y / (aHeight / height))] > 0)
            {
                flag1 = true;
                //Debug.Log("Flag");
            }
            else
            {
                flag1 = false;
                //Debug.Log("NoFlag");
            }
            if (flag1 == true && flag2 == false)
            {
                //Debug.Log("Flagged");
                startRiver = i;
            }
            if (flag1 == false && flag2 == true)
            {
                //Debug.Log("Unflagged");
                endRiver = i;
            }
            flag2 = flag1;
        }

        if (startRiver == -1 && endRiver == -1)
        {
            startRiver = 0;
            endRiver = 0;

        }
        else if (startRiver == -1 && endRiver != -1)
        {
            startRiver = 0;
        }
        else if (endRiver == -1 && startRiver != -1)
        {
            endRiver = points.Length;
        }

        int[] sidePathStartingPoints = new int[points.Length];

        // newValue determines the next starting point of a sidePath
        int newValue = startRiver - (int)Random.Range(minimalSidePathRiverDifference, maximalSidePathRiverDifference);
        int a = 0;

        // determine the startingpoints for the sidepaths before the river
        while (newValue > 0)
        {
            sidePathStartingPoints[a] = newValue;
            a++;
            newValue -= (int)Random.Range(minimalSidePathDifference, maximalSidePathDifference);
        }

        newValue = endRiver + (int)Random.Range(minimalSidePathRiverDifference, maximalSidePathRiverDifference);

        // determine the startingpoints for the sidepaths behind the river
        while (newValue < points.Length)
        {
            sidePathStartingPoints[a] = newValue;
            a++;
            newValue += (int)Random.Range(minimalSidePathDifference, maximalSidePathDifference);
        }

        for (int p = 0; p < sidePathStartingPoints.Length; p++)
        {
            if (sidePathStartingPoints[p + 1] == sidePathStartingPoints[p] || sidePathStartingPoints[p] == 0)
            {
                p = sidePathStartingPoints.Length;
            }
            else
            {

                //draw line from startpoint with the normal of the mainPath to the edge of the city/ to the river to determine an end point

                startPoint.x = points[sidePathStartingPoints[p]].x;
                startPoint.y = points[sidePathStartingPoints[p]].y;

                //Instantiate(cube, new Vector3(startPoint.y, 40, startPoint.x), Quaternion.identity);

                int dx = dx_normal;
                int dy = dy_normal;

                //rico from the normal, used by pathoffset
                int dx_offset = dy;
                int dy_offset = -dx;

                max = 0;

                // calculate the maximum delta
                if (abs(dx) < abs(dy)) max = abs(dy);
                else max = abs(dx);

                // bool firstTime = true;

                cord [] prevPoints = new cord[sidePathLengthReduction + 1];
                for (int n = 0; n <= max; n++)
                {
                    
                    int sidePathPointx = CheckAX(startPoint.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                    int sidePathPointy = CheckAY(startPoint.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));
                    
                    // if (firstTime)
                    // {
                    //     for (int i = 0; i <= sidePathLengthReduction; i++)
                    //     {
                    //         prev
                    //     }
                    // }

                    

                    for (int i = sidePathLengthReduction; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            prevPoints[i].x = prevPoints[i - 1].x;
                            prevPoints[i].y = prevPoints[i - 1].y;
                        }
                        else
                        {
                            prevPoints[i].x = sidePathPointx;
                            prevPoints[i].y = sidePathPointy;
                        }
                        Debug.Log("PrevPoint[i] = " + prevPoints[i]);
                    }

                    // use the point at the edge of the city as the endpoint of the sidepath
                    if (cityTerrain[(int)Mathf.Floor(sidePathPointx / (aWidth / width)), (int)Mathf.Floor(sidePathPointy / (aHeight / height))] != 1 || river[(int)Mathf.Floor(sidePathPointx / (aWidth / width)), (int)Mathf.Floor(sidePathPointy / (aHeight / height))] != 0)
                    {
                        endPoint.x = prevPoints[sidePathLengthReduction].x;
                        endPoint.y = prevPoints[sidePathLengthReduction].y;
                        //Instantiate(cube, new Vector3(endPoint.y, 40, endPoint.x), Quaternion.identity);
                        n = max + 1;

                    }
                    else if (n == max)
                    {
                        // if line doesn't go outise citybounds, use last point instead
                        endPoint.x = prevPoints[sidePathLengthReduction].x;
                        endPoint.y = prevPoints[sidePathLengthReduction].y;
                    }
                    
                }

                dx = endPoint.x - startPoint.x;
                dy = endPoint.y - startPoint.y;

                int r = (int)Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));

                if (r >= minimumSidePathLength)
                {

                    //rico from the normal, used by pathoffset
                    dx_offset = dy;
                    dy_offset = -dx;

                    max = 0;

                    // calculate the maximum delta 
                    if (abs(dx) < abs(dy)) max = abs(dy);
                    else max = abs(dx);

                    // generate an empty array of cords, the size of the total number of sidePathPoints
                    cord[] sidePathPoints = new cord[max + 1];


                    //generate points, which the path lies between
                    for (int n = 0; n <= max; n++)
                    {

                        //generaterate the coordinates of the points in between the start- and end-point, which makes a straight line
                        int sidePathPointx = CheckAX(startPoint.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                        int sidePathPointy = CheckAY(startPoint.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));

                        // Calculate the offset differrence, this is the difference between the delta height of the start and end point and
                        // the perlin noise height of the current point. This is to make sure the path starts and ends where the start and end
                        // points are and to make it meander more.
                        float sidePathOffsetDifference = ((CalculateHeight(0, max, sidePathperiod) - CalculateHeight(0, 0, sidePathperiod)) / max) * n + CalculateHeight(0, 0, sidePathperiod);
                        //calculate the offset for each point.
                        float sidePathoffset = sidePathOffsetFactor * (CalculateHeight(0, n, sidePathperiod) - sidePathOffsetDifference);

                        // calculate the offset for each point, this is used to calculate the xOffset and yOffset
                        float sidePathPointOffsetFactor = sidePathoffset / Mathf.Sqrt(Mathf.Pow((int)dx_offset, 2f) + Mathf.Pow((int)dy_offset, 2f));

                        //calculate the coordinates of the new pathPoints
                        sidePathPointx += (int)Mathf.Round((int)dx_offset * (float)sidePathPointOffsetFactor);
                        sidePathPointy += (int)Mathf.Round((int)dy_offset * (float)sidePathPointOffsetFactor);

                        //push them to the list of pathPoints
                        sidePathPoints[n] = new cord
                        {
                            x = sidePathPointx,
                            y = sidePathPointy
                        };

                        // Debug.Log("x: " + pathPoints[n].x);
                        // Debug.Log("y: " + pathPoints[n].y);

                    }

                    // 
                    //
                    //   Dit kan efficienter
                    //
                    //


                    for (int x = 0; x < aWidth; x++)
                    {
                        for (int y = 0; y < aHeight; y++)
                        {
                            if (path[x, y] != 0 || sidePath[x, y] != 0) SpaceMap[(int)((float)x * ((float)width / (float)aWidth)), (int)((float)y * ((float)height / (float)aHeight))] = false;
                        }
                    }




                    //
                    //
                    //
                    //
                    //

                    dx_dy_sidePathPoints = new cord[sidePathPoints.Length];

                    for (int i = 0; i < sidePathPoints.Length - 1; i++)
                    {
                        // Debug.Log(i + " " + pathPoints.Length);
                        dx = sidePathPoints[i + 1].x - sidePathPoints[i].x;
                        dy = sidePathPoints[i + 1].y - sidePathPoints[i].y;

                        //the distance between 2 points over 1 axis
                        int distance = 0;
                        // calculate the maximum delta
                        if (abs(dx) < abs(dy)) distance = abs(dy);
                        else distance = abs(dx);

                        // generate all sidepathpoints in cities
                        for (int n = 0; n <= distance; n++)
                        {

                            int sidePathPointx = CheckAX(sidePathPoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n)));
                            int sidePathPointy = CheckAY(sidePathPoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n)));

                            // check if sidePath stays inside city bounds
                            // if(cityTerrain[(int)Mathf.Floor(sidePathPointx/(aWidth/width)),(int)Mathf.Floor(sidePathPointy/(aHeight/height))]!=1){

                            //     sidePathGenerationAttempts++;

                            //     generateCitySidePaths(line, points, ref city, ref cityTerrain, ref path, ref river);
                            //     return;
                            // }

                            output[sidePathPointx, sidePathPointy] = 1;
                        }
                    }
                    for (int x = 0; x < aWidth; x++)
                    {
                        for (int y = 0; y < aHeight; y++)
                        {
                            // output filter is copied to sidePath filter
                            if (output[x, y] == 1) sidePath[x, y] = output[x, y];
                        }
                    }

                    // determine the points where the doors from the houses will be

                    int houseStartingPoint = Random.Range(minimalHouseMainPathDifference, maximalHouseMainPathDifference);

                    cord housePoint;

                    while (houseStartingPoint < sidePathPoints.Length)
                    {
                        housePoint = sidePathPoints[houseStartingPoint];


                        // determine the slope on every sidePathPoint with a certain precision
                        for (int i = 0; i < 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[0].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[0].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = 5; i < sidePathPoints.Length - 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = sidePathPoints.Length - 5; i < sidePathPoints.Length; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[sidePathPoints.Length - 1].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[sidePathPoints.Length - 1].y;
                        }

                        //place a house

                        Vector3 housePosition = new Vector3(housePoint.y / (aHeight / height), AverageHeight * depth, housePoint.x / (aWidth / width));

                        dx = -dx_dy_sidePathPoints[houseStartingPoint].y;
                        dy = dx_dy_sidePathPoints[houseStartingPoint].x;

                        //w(dx^2+dy^2)*a = r

                        //adx^2+ady^2=r^2
                        //a=w(r^2/(dx^2+dy^2))
                        //dx = w(r^2-dy^2)

                        float normalSlopeX;
                        float normalSlopeY;

                        normalSlopeX = dx;
                        normalSlopeY = dy;

                        float multiplyFactor = Mathf.Sqrt(Mathf.Pow((int)Random.Range(minHouseOffset, maxHouseOffset), 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePosition.x += normalSlopeY * multiplyFactor;
                        housePosition.z += normalSlopeX * multiplyFactor;

                        // formula: asin(dy/r)/PI*180
                        int houseRotation = (int)-(Mathf.Asin(dx_dy_sidePathPoints[houseStartingPoint].x / (Mathf.Sqrt(Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].x, 2) + Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].y, 2)))) / Mathf.PI * 180f);
                        int maxWidth = 10;
                        int maxDepth = 10;

                        int houseWidth = 5;
                        int houseDepth = 9;

                        

                        int dxPoints = -dx;
                        int dyPoints = dy;

                        multiplyFactor = -Mathf.Sqrt(Mathf.Pow(houseWidth, 2) / (Mathf.Pow(dxPoints, 2) + Mathf.Pow(dyPoints, 2)));

                        cord[] housePoints = new cord[10];

                        // // used later on to determine the offset of each housePoint, formula: w(r^2 / (dx^2 + dy^2))
                        // multiplyFactor = Mathf.Sqrt(Mathf.Pow((houseWidth)/2f - 1, 2) / (Mathf.Pow(dxPoints, 2) + Mathf.Pow(dyPoints, 2)));

                        // // min and maxCheck are determined by splitting houseWidth in half: one positive, one negative side, and a middlepoint (0)
                        // int minCheck = -Mathf.FloorToInt((houseWidth - 1) / 2);
                        // int maxCheck = Mathf.CeilToInt((houseWidth - 1) / 2);


                        // // the list of points used to determine if there is enough space to place a house
                        // cord[] housePoints = new cord[(maxCheck * 2 + 1) * 2];

                        // for(int i = minCheck; i <= maxCheck; i++)
                        // {
                        //     //
                        //     housePoints[i + maxCheck].x = (int)housePosition.z + (int)Mathf.RoundToInt(multiplyFactor * dyPoints * i);
                        //     housePoints[i + maxCheck].y = (int)housePosition.x + (int)Mathf.RoundToInt(multiplyFactor * dxPoints * i);//original cubes

                        //     Debug.Log("HousePoint: " + (i + maxCheck));
                        //     Debug.Log("MultiplyFactor: " + multiplyFactor);



                        //     GameObject cubous = Instantiate(cube, new Vector3(housePoints[i + maxCheck].y, 50f, housePoints[i + maxCheck].x), Quaternion.identity);
                        //     Color newColor = new Color(0.125f * (i+maxCheck), 0, 0, 1.0f);
                        //     cubous.GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
                        // }

                        // multiplyFactor = Mathf.Sqrt(Mathf.Pow(houseDepth, 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));
                        // int translationX = (int)(normalSlopeX * multiplyFactor);
                        // int translationY = (int)(normalSlopeY * multiplyFactor);


                        // for (int i = minCheck; i <= maxCheck; i++)
                        // {

                        //     housePoints[i + (2 * maxCheck + 3)].x = housePoints[i + maxCheck].x + translationX;//translated cube
                        //     housePoints[i + (2 * maxCheck + 3)].y = housePoints[i + maxCheck].y + translationY;

                        //     Debug.Log("TransHousePoint: " + (i + (2 * maxCheck + 3)));
                        //     Debug.Log("OPX: " + housePoints[i + maxCheck].x);
                        //     Debug.Log("OPY: " + housePoints[i + maxCheck].y);
                        //     Debug.Log("TPX: " + housePoints[i + (2 * maxCheck + 3)].x);
                        //     Debug.Log("TPY: " + housePoints[i + (2 * maxCheck + 3)].y);

                        //     Debug.Log("OriginHousePoint: " + (i + maxCheck));
                        //     Debug.Log("MultiplyFactor: " + multiplyFactor);



                        //     GameObject cubous = Instantiate(cube, new Vector3(housePoints[i + (2 * maxCheck + 3)].y, 50f, housePoints[i + (2 * maxCheck + 3)].x), Quaternion.identity);
                        //     Color newColor = new Color(0.125f * (i+maxCheck), 0, 0, 1.0f);
                        //     cubous.GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);

                        //     //ik ben hier bezig
                        // }

                        

                        housePoints[0].x = (int)housePosition.z + (int)(multiplyFactor * dyPoints);
                        housePoints[0].y = (int)housePosition.x + (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[0].y, 50f, housePoints[0].x), Quaternion.identity);

                        housePoints[1].x = (int)housePosition.z;
                        housePoints[1].y = (int)housePosition.x;

                        //Instantiate(cube, new Vector3(housePoints[1].y, 50f, housePoints[1].x), Quaternion.identity);

                        housePoints[2].x = (int)housePosition.z - (int)(multiplyFactor * dyPoints);
                        housePoints[2].y = (int)housePosition.x - (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[2].y, 50f, housePoints[2].x), Quaternion.identity);

                        multiplyFactor = Mathf.Sqrt(Mathf.Pow(houseDepth, 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePoints[3].x = housePoints[0].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[3].y = housePoints[0].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[3].y, 50f, housePoints[3].x), Quaternion.identity);

                        housePoints[4].x = housePoints[1].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[4].y = housePoints[1].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[4].y, 50f, housePoints[4].x), Quaternion.identity);

                        housePoints[5].x = housePoints[2].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[5].y = housePoints[2].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[5].y, 50f, housePoints[5].x), Quaternion.identity);

                        //the distance between 2 points over 1 axis
                        int distance = 0;

                        bool placeHouse = true;

                        for (int i = 0; i < housePoints.Length/2; i++)
                        {

                            dx = housePoints[i + housePoints.Length/2].x - housePoints[i].x;
                            dy = housePoints[i + housePoints.Length/2].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }


                        for (int i = 0; i < 5; i++)
                        {
                            dx = housePoints[i + 1].x - housePoints[i].x;
                            dy = housePoints[i + 1].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }


                        if (placeHouse)
                        {
                            housegenerator.GenerateHouse(housePosition, houseRotation, maxWidth, maxDepth);

                            // set SpaceMap values to false at the location of the house
                            for (int i = 0; i < 3; i++)
                            {
                                dx = housePoints[i + 3].x - housePoints[i].x;
                                dx = housePoints[i + 3].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                dx = housePoints[i + 1].x - housePoints[i].x;
                                dy = housePoints[i + 1].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }
                        }

                        houseStartingPoint += (int)Random.Range(minimalHouseDifference, maximalHouseDifference);

                    }

                    //generate houses on other side of the sidePath

                    // determine the points where the doors from the houses will be

                    houseStartingPoint = Random.Range(minimalHouseMainPathDifference, maximalHouseMainPathDifference);

                    while (houseStartingPoint < sidePathPoints.Length)
                    {
                        housePoint = sidePathPoints[houseStartingPoint];


                        // determine the slope on every sidePathPoint with a certain precision
                        for (int i = 0; i < 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[0].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[0].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = 5; i < sidePathPoints.Length - 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = sidePathPoints.Length - 5; i < sidePathPoints.Length; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[sidePathPoints.Length - 1].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[sidePathPoints.Length - 1].y;
                        }

                        //place a house

                        Vector3 housePosition = new Vector3(housePoint.y / (aHeight / height), AverageHeight * depth, housePoint.x / (aWidth / width));

                        dx = dx_dy_sidePathPoints[houseStartingPoint].y;
                        dy = -dx_dy_sidePathPoints[houseStartingPoint].x;

                        //w(dx^2+dy^2)*a = r

                        //adx^2+ady^2=r^2
                        //a=w(r^2/(dx^2+dy^2))
                        //dx = w(r^2-dy^2)

                        float normalSlopeX;
                        float normalSlopeY;

                        normalSlopeX = dx;
                        normalSlopeY = dy;

                        float multiplyFactor = Mathf.Sqrt(Mathf.Pow((int)Random.Range(minHouseOffset, maxHouseOffset), 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePosition.x += normalSlopeY * multiplyFactor;
                        housePosition.z += normalSlopeX * multiplyFactor;

                        // formula: asin(dy/r)/PI*180
                        int houseRotation = (int)-(Mathf.Asin(dx_dy_sidePathPoints[houseStartingPoint].x / (Mathf.Sqrt(Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].x, 2) + Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].y, 2)))) / Mathf.PI * 180f + 180f);
                        int maxWidth = 10;
                        int maxDepth = 10;

                        cord[] housePoints = new cord[10];

                        int houseWidth = 5;
                        int houseDepth = 9;

                        int dxPoints = -dx;
                        int dyPoints = dy;

                        multiplyFactor = Mathf.Sqrt(Mathf.Pow(houseWidth, 2) / (Mathf.Pow(dxPoints, 2) + Mathf.Pow(dyPoints, 2)));

                        housePoints[0].x = (int)housePosition.z + (int)(multiplyFactor * dyPoints);
                        housePoints[0].y = (int)housePosition.x + (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[0].y, 50f, housePoints[0].x), Quaternion.identity);

                        housePoints[1].x = (int)housePosition.z;
                        housePoints[1].y = (int)housePosition.x;

                        //Instantiate(cube, new Vector3(housePoints[1].y, 50f, housePoints[1].x), Quaternion.identity);

                        housePoints[2].x = (int)housePosition.z - (int)(multiplyFactor * dyPoints);
                        housePoints[2].y = (int)housePosition.x - (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[2].y, 50f, housePoints[2].x), Quaternion.identity);

                        multiplyFactor = Mathf.Sqrt(Mathf.Pow(houseDepth, 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePoints[3].x = housePoints[0].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[3].y = housePoints[0].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[3].y, 50f, housePoints[3].x), Quaternion.identity);

                        housePoints[4].x = housePoints[1].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[4].y = housePoints[1].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[4].y, 50f, housePoints[4].x), Quaternion.identity);

                        housePoints[5].x = housePoints[2].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[5].y = housePoints[2].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[5].y, 50f, housePoints[5].x), Quaternion.identity);

                        //the distance between 2 points over 1 axis
                        int distance = 0;

                        bool placeHouse = true;

                        for (int i = 0; i < 3; i++)
                        {

                            dx = housePoints[i + 3].x - housePoints[i].x;
                            dy = housePoints[i + 3].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }


                        for (int i = 0; i < 5; i++)
                        {
                            dx = housePoints[i + 1].x - housePoints[i].x;
                            dy = housePoints[i + 1].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }

                        if (placeHouse)
                        {
                            housegenerator.GenerateHouse(housePosition, houseRotation, maxWidth, maxDepth);

                            // set SpaceMap values to false at the location of the house
                            for (int i = 0; i < 3; i++)
                            {
                                dx = housePoints[i + 3].x - housePoints[i].x;
                                dx = housePoints[i + 3].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                dx = housePoints[i + 1].x - housePoints[i].x;
                                dy = housePoints[i + 1].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }
                        }

                        houseStartingPoint += (int)Random.Range(minimalHouseDifference, maximalHouseDifference);

                    }

                }
            }
        }


        // generating sidepaths on the other side of the main path

        sidePathStartingPoints = new int[points.Length];

        // newValue determines the next starting point of a sidePath
        newValue = startRiver - (int)Random.Range(minimalSidePathRiverDifference, maximalSidePathRiverDifference);
        a = 0;

        // determine the startingpoints for the sidepaths before the river
        while (newValue > 0)
        {
            sidePathStartingPoints[a] = newValue;
            a++;
            newValue -= (int)Random.Range(minimalSidePathDifference, maximalSidePathDifference);
        }

        newValue = endRiver + (int)Random.Range(minimalSidePathRiverDifference, maximalSidePathRiverDifference);

        // determine the startingpoints for the sidepaths behind the river
        while (newValue < points.Length)
        {
            sidePathStartingPoints[a] = newValue;
            a++;
            newValue += (int)Random.Range(minimalSidePathDifference, maximalSidePathDifference);
        }

        for (int p = 0; p < sidePathStartingPoints.Length; p++)
        {
            if (sidePathStartingPoints[p + 1] == sidePathStartingPoints[p] || sidePathStartingPoints[p] == 0)
            {
                p = sidePathStartingPoints.Length;
            }
            else
            {

                //draw line between city points for referance
                startPoint.x = points[sidePathStartingPoints[p]].x;
                startPoint.y = points[sidePathStartingPoints[p]].y;

                //Instantiate(cube, new Vector3(startPoint.y, 40, startPoint.x), Quaternion.identity);

                int dx = -dx_normal;
                int dy = -dy_normal;

                //rico from the normal, used by pathoffset
                int dx_offset = dy;
                int dy_offset = -dx;

                max = 0;

                // calculate the maximum delta 
                if (abs(dx) < abs(dy)) max = abs(dy);
                else max = abs(dx);

                cord [] prevPoints = new cord[sidePathLengthReduction + 1];

                for (int n = 0; n <= max; n++)
                {

                    int sidePathPointx = CheckAX(startPoint.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                    int sidePathPointy = CheckAY(startPoint.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));
                    
                    

                    for (int i = sidePathLengthReduction; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            prevPoints[i].x = prevPoints[i - 1].x;
                            prevPoints[i].y = prevPoints[i - 1].y;
                        }
                        else
                        {
                            prevPoints[i].x = sidePathPointx;
                            prevPoints[i].y = sidePathPointy;
                        }
                        Debug.Log("PrevPoint[i] = " + prevPoints[i]);
                    }

                    // use the point at the edge of the city as the endpoint of the sidepath
                    if (cityTerrain[(int)Mathf.Floor(sidePathPointx / (aWidth / width)), (int)Mathf.Floor(sidePathPointy / (aHeight / height))] != 1 || river[(int)Mathf.Floor(sidePathPointx / (aWidth / width)), (int)Mathf.Floor(sidePathPointy / (aHeight / height))] != 0)
                    {
                        endPoint.x = prevPoints[sidePathLengthReduction].x;
                        endPoint.y = prevPoints[sidePathLengthReduction].y;
                        //Instantiate(cube, new Vector3(endPoint.y, 40, endPoint.x), Quaternion.identity);
                        n = max + 1;

                    }
                    else if (n == max)
                    {
                        // if line doesn't go outise citybounds, use last point instead
                        endPoint.x = prevPoints[sidePathLengthReduction].x;
                        endPoint.y = prevPoints[sidePathLengthReduction].y;
                    }
                }

                dx = endPoint.x - startPoint.x;
                dy = endPoint.y - startPoint.y;

                int r = (int)Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));

                if (r >= minimumSidePathLength)
                {

                    //rico from the normal, used by pathoffset
                    dx_offset = dy;
                    dy_offset = -dx;

                    max = 0;

                    // calculate the maximum delta 
                    if (abs(dx) < abs(dy)) max = abs(dy);
                    else max = abs(dx);

                    // generate an empty array of cords, the size of the total number of sidePathPoints
                    cord[] sidePathPoints = new cord[max + 1];

                    //generate points, which the path lies between
                    for (int n = 0; n <= max; n++)
                    {

                        //generaterate the coordinates of the points in between the start- and end-point, which makes a straight line
                        int sidePathPointx = CheckAX(startPoint.x + (int)Mathf.Round(((float)dx / (float)max * (float)n)));
                        int sidePathPointy = CheckAY(startPoint.y + (int)Mathf.Round(((float)dy / (float)max * (float)n)));

                        // Calculate the offset differrence, this is the difference between the delta height of the start and end point and
                        // the perlin noise height of the current point. This is to make sure the path starts and ends where the start and end
                        // points are and to make it meander more.
                        float sidePathOffsetDifference = ((CalculateHeight(0, max, sidePathperiod) - CalculateHeight(0, 0, sidePathperiod)) / max) * n + CalculateHeight(0, 0, sidePathperiod);
                        //calculate the offset for each point.
                        float sidePathoffset = sidePathOffsetFactor * (CalculateHeight(0, n, sidePathperiod) - sidePathOffsetDifference);

                        // calculate the offset for each point, this is used to calculate the xOffset and yOffset
                        float sidePathPointOffsetFactor = sidePathoffset / Mathf.Sqrt(Mathf.Pow((int)dx_offset, 2f) + Mathf.Pow((int)dy_offset, 2f));

                        //calculate the coordinates of the new pathPoints
                        sidePathPointx += (int)Mathf.Round((int)dx_offset * (float)sidePathPointOffsetFactor);
                        sidePathPointy += (int)Mathf.Round((int)dy_offset * (float)sidePathPointOffsetFactor);

                        //push them to the list of pathPoints
                        sidePathPoints[n] = new cord
                        {
                            x = sidePathPointx,
                            y = sidePathPointy
                        };

                        // Debug.Log("x: " + pathPoints[n].x);
                        // Debug.Log("y: " + pathPoints[n].y);

                    }

                    // for (int x = 0; x < aWidth; x++)
                    // {
                    //     for (int y = 0; y < aHeight; y++)
                    //     {
                    //         if (path[x, y] != 0 || sidePath[x, y] != 0) SpaceMap[(int)((float)x * ((float)width / (float)aWidth)), (int)((float)y * ((float)height / (float)aHeight))] = false;
                    //     }
                    // }

                    dx_dy_sidePathPoints = new cord[sidePathPoints.Length];

                    for (int i = 0; i < sidePathPoints.Length - 1; i++)
                    {
                        // Debug.Log(i + " " + pathPoints.Length);
                        dx = sidePathPoints[i + 1].x - sidePathPoints[i].x;
                        dy = sidePathPoints[i + 1].y - sidePathPoints[i].y;

                        //dx_dy_sidePathPoints[i].x = dx;
                        //dx_dy_sidePathPoints[i].y = dy;

                        //the distance between 2 points over 1 axis
                        int distance = 0;
                        // calculate the maximum delta
                        if (abs(dx) < abs(dy)) distance = abs(dy);
                        else distance = abs(dx);

                        // generate all sidepathpoints in cities
                        for (int n = 0; n <= distance; n++)
                        {

                            int sidePathPointx = CheckAX(sidePathPoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n)));
                            int sidePathPointy = CheckAY(sidePathPoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n)));




                            // check if sidePath stays inside city bounds
                            // if(cityTerrain[(int)Mathf.Floor(sidePathPointx/(aWidth/width)),(int)Mathf.Floor(sidePathPointy/(aHeight/height))]!=1){

                            //     sidePathGenerationAttempts++;

                            //     generateCitySidePaths(line, points, ref city, ref cityTerrain, ref path, ref river);
                            //     return;
                            // }

                            output[sidePathPointx, sidePathPointy] = 1;
                        }
                    }

                    for (int x = 0; x < aWidth; x++)
                    {
                        for (int y = 0; y < aHeight; y++)
                        {
                            // output filter is copied to sidePath filter
                            if (output[x, y] == 1) sidePath[x, y] = output[x, y];
                        }
                    }

                    for (int x = 0; x < aWidth; x++)
                    {
                        for (int y = 0; y < aHeight; y++)
                        {
                            if (path[x, y] != 0 || sidePath[x, y] != 0) SpaceMap[(int)((float)x * ((float)width / (float)aWidth)), (int)((float)y * ((float)height / (float)aHeight))] = false;
                        }
                    }


                    // determine the points where the doors from the houses will be

                    int houseStartingPoint = Random.Range(minimalHouseMainPathDifference, maximalHouseMainPathDifference); ;

                    cord housePoint;

                    while (houseStartingPoint < sidePathPoints.Length)
                    {
                        housePoint = sidePathPoints[houseStartingPoint];


                        // determine the slope on every sidePathPoint with a certain precision
                        for (int i = 0; i < 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[0].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[0].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = 5; i < sidePathPoints.Length - 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = sidePathPoints.Length - 5; i < sidePathPoints.Length; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[sidePathPoints.Length - 1].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[sidePathPoints.Length - 1].y;
                        }

                        //place a house

                        Vector3 housePosition = new Vector3(housePoint.y / (aHeight / height), AverageHeight * depth, housePoint.x / (aWidth / width));

                        dx = -dx_dy_sidePathPoints[houseStartingPoint].y;
                        dy = dx_dy_sidePathPoints[houseStartingPoint].x;



                        //w(dx^2+dy^2)*a = r

                        //adx^2+ady^2=r^2
                        //a=w(r^2/(dx^2+dy^2))
                        //dx = w(r^2-dy^2)

                        float normalSlopeX;
                        float normalSlopeY;



                        normalSlopeX = dx;
                        normalSlopeY = dy;



                        // r = 3
                        // dy = 9


                        float multiplyFactor = -Mathf.Sqrt(Mathf.Pow((int)Random.Range(minHouseOffset, maxHouseOffset), 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        //Debug.Log("SlopeX: " + normalSlopeX);
                        //Debug.Log("SlopeY: " + normalSlopeY);

                        housePosition.x += normalSlopeY * multiplyFactor;
                        housePosition.z += normalSlopeX * multiplyFactor;

                        // formula: asin(dy/r)/PI*180
                        int houseRotation = (int)(Mathf.Asin(dx_dy_sidePathPoints[houseStartingPoint].x / (Mathf.Sqrt(Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].x, 2) + Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].y, 2)))) / Mathf.PI * 180f);
                        int maxWidth = 10;
                        int maxDepth = 10;





                        cord[] housePoints = new cord[10];



                        int dxPoints = -dx;
                        int dyPoints = dy;

                        multiplyFactor = -Mathf.Sqrt(Mathf.Pow(houseWidth, 2) / (Mathf.Pow(dxPoints, 2) + Mathf.Pow(dyPoints, 2)));

                        housePoints[0].x = (int)housePosition.z + (int)(multiplyFactor * dyPoints);
                        housePoints[0].y = (int)housePosition.x + (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[0].y, 50f, housePoints[0].x), Quaternion.identity);

                        housePoints[1].x = (int)housePosition.z;
                        housePoints[1].y = (int)housePosition.x;

                        //Instantiate(cube, new Vector3(housePoints[1].y, 50f, housePoints[1].x), Quaternion.identity);

                        housePoints[2].x = (int)housePosition.z - (int)(multiplyFactor * dyPoints);
                        housePoints[2].y = (int)housePosition.x - (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[2].y, 50f, housePoints[2].x), Quaternion.identity);

                        multiplyFactor = -Mathf.Sqrt(Mathf.Pow(houseDepth, 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePoints[3].x = housePoints[0].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[3].y = housePoints[0].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[3].y, 50f, housePoints[3].x), Quaternion.identity);

                        housePoints[4].x = housePoints[1].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[4].y = housePoints[1].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[4].y, 50f, housePoints[4].x), Quaternion.identity);

                        housePoints[5].x = housePoints[2].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[5].y = housePoints[2].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[5].y, 50f, housePoints[5].x), Quaternion.identity);

                        //the distance between 2 points over 1 axis
                        int distance = 0;

                        bool placeHouse = true;

                        for (int i = 0; i < 3; i++)
                        {

                            dx = housePoints[i + 3].x - housePoints[i].x;
                            dy = housePoints[i + 3].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }

                        for (int i = 0; i < 5; i++)
                        {


                            dx = housePoints[i + 1].x - housePoints[i].x;
                            dy = housePoints[i + 1].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }




                        if (placeHouse)
                        {
                            housegenerator.GenerateHouse(housePosition, houseRotation, maxWidth, maxDepth);

                            // set SpaceMap values to false at the location of the house
                            for (int i = 0; i < 3; i++)
                            {
                                dx = housePoints[i + 3].x - housePoints[i].x;
                                dx = housePoints[i + 3].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                dx = housePoints[i + 1].x - housePoints[i].x;
                                dy = housePoints[i + 1].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }
                        }

                        houseStartingPoint += (int)Random.Range(minimalHouseDifference, maximalHouseDifference);

                    }



                    // determine the points where the doors from the houses will be on the other side of the sidePath

                    houseStartingPoint = Random.Range(minimalHouseMainPathDifference, maximalHouseMainPathDifference); ;

                    while (houseStartingPoint < sidePathPoints.Length)
                    {
                        housePoint = sidePathPoints[houseStartingPoint];


                        // determine the slope on every sidePathPoint with a certain precision
                        for (int i = 0; i < 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[0].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[0].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = 5; i < sidePathPoints.Length - 5; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[i + 5].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[i + 5].y;
                        }

                        for (int i = sidePathPoints.Length - 5; i < sidePathPoints.Length; i++)
                        {
                            dx_dy_sidePathPoints[i].x = sidePathPoints[i - 5].x - sidePathPoints[sidePathPoints.Length - 1].x;
                            dx_dy_sidePathPoints[i].y = sidePathPoints[i - 5].y - sidePathPoints[sidePathPoints.Length - 1].y;
                        }

                        //place a house

                        Vector3 housePosition = new Vector3(housePoint.y / (aHeight / height), AverageHeight * depth, housePoint.x / (aWidth / width));

                        dx = dx_dy_sidePathPoints[houseStartingPoint].y;
                        dy = -dx_dy_sidePathPoints[houseStartingPoint].x;



                        //w(dx^2+dy^2)*a = r

                        //adx^2+ady^2=r^2
                        //a=w(r^2/(dx^2+dy^2))
                        //dx = w(r^2-dy^2)

                        float normalSlopeX;
                        float normalSlopeY;



                        normalSlopeX = dx;
                        normalSlopeY = dy;



                        // r = 3
                        // dy = 9


                        float multiplyFactor = -Mathf.Sqrt(Mathf.Pow((int)Random.Range(minHouseOffset, maxHouseOffset), 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        //Debug.Log("SlopeX: " + normalSlopeX);
                        //Debug.Log("SlopeY: " + normalSlopeY);

                        housePosition.x += normalSlopeY * multiplyFactor;
                        housePosition.z += normalSlopeX * multiplyFactor;

                        // formula: asin(dy/r)/PI*180
                        int houseRotation = (int)(Mathf.Asin(dx_dy_sidePathPoints[houseStartingPoint].x / (Mathf.Sqrt(Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].x, 2) + Mathf.Pow(dx_dy_sidePathPoints[houseStartingPoint].y, 2)))) / Mathf.PI * 180f + 180f);
                        int maxWidth = 10;
                        int maxDepth = 10;





                        cord[] housePoints = new cord[10];

                        int houseWidth = 5;
                        int houseDepth = 9;

                        int dxPoints = -dx;
                        int dyPoints = dy;

                        multiplyFactor = -Mathf.Sqrt(Mathf.Pow(houseWidth, 2) / (Mathf.Pow(dxPoints, 2) + Mathf.Pow(dyPoints, 2)));

                        housePoints[0].x = (int)housePosition.z + (int)(multiplyFactor * dyPoints);
                        housePoints[0].y = (int)housePosition.x + (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[0].y, 50f, housePoints[0].x), Quaternion.identity);

                        housePoints[1].x = (int)housePosition.z;
                        housePoints[1].y = (int)housePosition.x;

                        //Instantiate(cube, new Vector3(housePoints[1].y, 50f, housePoints[1].x), Quaternion.identity);

                        housePoints[2].x = (int)housePosition.z - (int)(multiplyFactor * dyPoints);
                        housePoints[2].y = (int)housePosition.x - (int)(multiplyFactor * dxPoints);

                        //Instantiate(cube, new Vector3(housePoints[2].y, 50f, housePoints[2].x), Quaternion.identity);

                        multiplyFactor = -Mathf.Sqrt(Mathf.Pow(houseDepth, 2) / (Mathf.Pow(normalSlopeX, 2) + Mathf.Pow(normalSlopeY, 2)));

                        housePoints[3].x = housePoints[0].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[3].y = housePoints[0].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[3].y, 50f, housePoints[3].x), Quaternion.identity);

                        housePoints[4].x = housePoints[1].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[4].y = housePoints[1].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[4].y, 50f, housePoints[4].x), Quaternion.identity);

                        housePoints[5].x = housePoints[2].x + (int)(normalSlopeX * multiplyFactor);
                        housePoints[5].y = housePoints[2].y + (int)(normalSlopeY * multiplyFactor);

                        //Instantiate(cube, new Vector3(housePoints[5].y, 50f, housePoints[5].x), Quaternion.identity);

                        //the distance between 2 points over 1 axis
                        int distance = 0;

                        bool placeHouse = true;

                        for (int i = 0; i < 3; i++)
                        {

                            dx = housePoints[i + 3].x - housePoints[i].x;
                            dy = housePoints[i + 3].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }



                        for (int i = 0; i < 5; i++)
                        {

                            dx = housePoints[i + 1].x - housePoints[i].x;
                            dy = housePoints[i + 1].y - housePoints[i].y;

                            // calculate the maximum delta
                            if (abs(dx) < abs(dy)) distance = abs(dy);
                            else distance = abs(dx);

                            // generate all sidepathpoints in cities
                            for (int n = 0; n <= distance; n++)
                            {

                                int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                if (houseCheckX < 0 || houseCheckX > width - 1 || houseCheckY < 0 || houseCheckY > height - 1)
                                {
                                    placeHouse = false;
                                }
                                else if (SpaceMap[houseCheckX, houseCheckY] == false)
                                {
                                    placeHouse = false;
                                }

                                //Instantiate(cube, new Vector3(houseCheckY, 40f, houseCheckX), Quaternion.identity);
                            }
                        }

                        if (placeHouse)
                        {
                            housegenerator.GenerateHouse(housePosition, houseRotation, maxWidth, maxDepth);

                            // set SpaceMap values to false at the location of the house
                            for (int i = 0; i < 3; i++)
                            {
                                dx = housePoints[i + 3].x - housePoints[i].x;
                                dx = housePoints[i + 3].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                dx = housePoints[i + 1].x - housePoints[i].x;
                                dy = housePoints[i + 1].y - housePoints[i].y;

                                // calculate the maximum delta
                                if (abs(dx) < abs(dy)) distance = abs(dy);
                                else distance = abs(dx);

                                // generate all sidepathpoints in cities
                                for (int n = 0; n <= distance; n++)
                                {
                                    int houseCheckX = (int)housePoints[i].x + (int)Mathf.Round(((float)dx / (float)distance * (float)n));
                                    int houseCheckY = (int)housePoints[i].y + (int)Mathf.Round(((float)dy / (float)distance * (float)n));

                                    if (houseCheckX >= 0 && houseCheckX < width && houseCheckY >= 0 && houseCheckY < height)
                                    {
                                        SpaceMap[houseCheckX, houseCheckY] = false;
                                    }
                                }
                            }
                        }

                        houseStartingPoint += (int)Random.Range(minimalHouseDifference, maximalHouseDifference);

                    }
                }
            }
        }
    }

    public int modulo(int number, int mod)
    {
        int value = number;
        while (value >= mod)
        {
            value -= mod;
        }
        while (value < 0)
        {
            value += mod;
        }
        return value;
    }
}