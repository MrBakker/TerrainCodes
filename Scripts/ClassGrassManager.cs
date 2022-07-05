using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrassSystem{
    public class GrassManager{
        Terrain terrain;
        float width, height, dWidth, dHeight;
        int[,,] map;

        private int CheckX(int x){
            if (x < 0) return 0;
            if (x >= dWidth) return (int)dWidth - 1;
            return x;
        }

        private int CheckY(int y){
            if (y < 0) return 0;
            if (y >= dHeight) return (int)dHeight - 1;
            return y;
        }

        public void SetUp(){
            terrain = Terrain.activeTerrain;
            dWidth = terrain.terrainData.detailWidth;
            dHeight = terrain.terrainData.detailHeight;
            Debug.Log("dwidth: " + dWidth + " dheight: " + dHeight);
            map = new int[5, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight];
        }

        public void SetTerrainSize(int width, int height){
            this.width = (float)width;
            this.height = (float)height;
        }

        public void SetTerrain(Terrain terrain){
            this.terrain = terrain;
        }

        public void AddGrass(Vector3 inputposition){
            int rand = Random.Range(0, 5);
            map[rand, (int)(inputposition.x * ((float)dWidth / (float)width)), (int)(inputposition.z * ((float)dHeight / (float)height))] = 1;
        }

        public void PrintGrass(){
            for (int i = 0; i < 5; i++){
                int[,] tijdelijk = new int[(int)dWidth, (int)dHeight];
                for (int x = 0; x < dWidth; x++){
                    for (int y = 0; y < dHeight; y++){
                        tijdelijk[x, y] = map[i, x, y];
                    }
                }
                terrain.terrainData.SetDetailLayer(0, 0, i, tijdelijk);
            }
        }
    };
}