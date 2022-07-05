using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeSystem{
    public class TreeManager{
        List<TreeInstance> instances;
        Terrain terrain;
        float width, height;
        
        public TreeManager(){
            instances = new List<TreeInstance>();
        }

        public void SetTerrainSize(int width, int height){
            this.width = (float)width;
            this.height = (float)height;
        }

        public void SetTerrain(Terrain terrain){
            this.terrain = terrain;
        }

        public void AddTree(Vector3 inputposition){
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(inputposition.x + 1, 110f, inputposition.z + 1), new Vector3(0f, -1f, 0f), out hit, 100f)) return;
            if (Physics.Raycast(new Vector3(inputposition.x + 1, 110f, inputposition.z - 1), new Vector3(0f, -1f, 0f), out hit, 100f)) return;
            if (Physics.Raycast(new Vector3(inputposition.x - 1, 110f, inputposition.z - 1), new Vector3(0f, -1f, 0f), out hit, 100f)) return;
            if (Physics.Raycast(new Vector3(inputposition.x - 1, 110f, inputposition.z + 1), new Vector3(0f, -1f, 0f), out hit, 100f)) return;
            TreeInstance treeTemp = new TreeInstance();
            treeTemp.position = new Vector3(inputposition.x / width, inputposition.y, inputposition.z / height);
            treeTemp.prototypeIndex = 0;
            treeTemp.widthScale = 1f;
            treeTemp.heightScale = 1f;
            treeTemp.color = Color.white;
            treeTemp.lightmapColor = Color.white;
            instances.Add(treeTemp);
            
        }

        public void PrintTrees(){
            terrain.terrainData.treeInstances = instances.ToArray();
        }
    };
}