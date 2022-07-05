using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    public Terrain t;
    

    // Blend the two terrain textures according to the steepness of
    // the slope at each point.
    void Start()
    {
        
        float[,,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, 2];

        // For each point on the alphamap...
        for (int y = 0; y < t.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < t.terrainData.alphamapWidth; x++)
            {
                // Get the normalized terrain coordinate that
                // corresponds to the the point.
                float normX = x * 1.0f / (t.terrainData.alphamapWidth - 1);
                float normY = y * 1.0f / (t.terrainData.alphamapHeight - 1);

                // Get the steepness value at the normalized coordinate.
                var angle = t.terrainData.GetSteepness(normX, normY);

                // Steepness is given as an angle, 0..90 degrees. Divide
                // by 90 to get an alpha blending value in the range 0..1.
                var frac = angle / 90.0;
                map[x, y, 0] = (float)frac;
                map[x, y, 1] = (float)(1 - frac);
            }
        }
        t.terrainData.SetAlphamaps(0, 0, map);
    }

    

        void AbsPaintTerrain(int x, int y, int texture) {

            float[,,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, 2];
            map[x, y, 0] = (float)0;
            map[x, y, 1] = (float)0;
            map[x, y, texture] = (float)1;

        }
}
