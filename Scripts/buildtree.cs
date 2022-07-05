using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildtree : MonoBehaviour
{
    public Terrain t;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("grass meuk");
        var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);
        Debug.Log(t.terrainData.detailHeight);
        Debug.Log(t.terrainData.detailWidth);

        // For each pixel in the detail map...
        for (var y = 0; y < t.terrainData.detailHeight; y++)
        {
            for (var x = 0; x < t.terrainData.detailWidth; x++)
            {
                map[x, y] = 0;
            }
        }

        for (int i = 0; i < 100; i++)
        {
            for (int o = 0; o < 100; o++)
            {
                map[i, o] = 1;
            }
        }

        // Assign the modified map back.
        t.terrainData.SetDetailLayer(0, 0, 0, map);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
