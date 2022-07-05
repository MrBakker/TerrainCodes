using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseSystem{
	public class HouseGenerator{
        private GameObject[] prefabs;
        private GameObject city;
        private int houses;
        private struct HouseInfo{
            public int id;
            public float depthdiff, heightdiff;

            public HouseInfo(int id, float depthdiff, float heightdiff) {
                this.id = id;
                this.depthdiff = depthdiff;
                this.heightdiff = heightdiff;
            }
        };

        private HouseInfo[] edge = new[]{
            new HouseInfo(157, 0f, 0f),
            new HouseInfo(15, 0.4f, -0.6f),
            new HouseInfo(-1, 0f, 0f)
            
        };

        private HouseInfo[] center = new[]{
            new HouseInfo(23, 0.4f, -0.6f),
            new HouseInfo(15, 0.4f, -0.6f),
            new HouseInfo(-1, 0f, 0f)
        };

        private const int TOTALADDONS = 3;

        public void SetUp(){
            prefabs = Resources.LoadAll<GameObject>("Buildings/Prefabs");
            city = new GameObject("All Buildings");
            houses = 0;
        }

        public bool GenerateHouse(Vector3 position, int rotation, int maxwidth, int maxdepth){
            if (maxwidth <= 5 || maxdepth <= 7) return false;

            houses++;
            GameObject newObj = new GameObject("House" + houses.ToString());
            newObj.transform.SetParent(city.transform);
            newObj.tag = "House";
            
            int type = Random.Range(0, 1), rand;

            if(type == 0){
                float[] xdiff = new float[6]{2.6f, 2.6f, -2.6f, -2.6f, -2.6f, 2.6f};
                float[] zdiff = new float[6]{-6.51f, -1.21f, -6.51f, -1.21f, -3.74f, -3.74f};
                float[] factor = new float[6]{1, 1, -1, -1, -1, 1};

                GameObject addon;
                //Build base
                GameObject base1child = UnityEngine.Object.Instantiate(prefabs[0], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                GameObject base2child = UnityEngine.Object.Instantiate(prefabs[0], new Vector3(0,0,0), Quaternion.identity) as GameObject;

                base1child.transform.SetParent(newObj.transform);
                base1child.transform.position = new Vector3(0f, 0f, -2.11f);
                base1child.name = "House" + houses.ToString() + " - Main_Building1";
                // base1child.AddComponent<BoxCollider>();
                base2child.transform.SetParent(newObj.transform);
                base2child.transform.position = new Vector3(0f, 0f, -5.53f);
                base2child.transform.Rotate(0f, 180.0f, 0.0f, Space.Self);
                base2child.name = "House" + houses.ToString() + " - Main_Building2";
                // base2child.AddComponent<BoxCollider>();

                //Build addons
                for (int i = 0; i < 4; i++){
                    rand = Random.Range(0, TOTALADDONS);
                    if (edge[rand].id != -1){
                        addon = UnityEngine.Object.Instantiate(prefabs[edge[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                        addon.transform.SetParent(newObj.transform);
                        addon.transform.position = new Vector3(xdiff[i] + edge[rand].depthdiff * factor[i], 2.1f + edge[rand].heightdiff, zdiff[i]);
                        addon.transform.Rotate(0f, 90.0f * factor[i], 0.0f, Space.Self);
                        // addon.AddComponent<BoxCollider>();
                    }
                }
                for (int i = 4; i < 6; i++){
                    rand = Random.Range(0, TOTALADDONS);
                    if (center[rand].id != -1){
                        addon = UnityEngine.Object.Instantiate(prefabs[center[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                        addon.transform.SetParent(newObj.transform);
                        addon.transform.position = new Vector3(xdiff[i] + center[rand].depthdiff * factor[i], 2.1f + center[rand].heightdiff, zdiff[i]);
                        addon.transform.Rotate(0f, 90.0f * factor[i], 0.0f, Space.Self);
                        // addon.AddComponent<BoxCollider>();
                    }
                }
                newObj.transform.Rotate(0f, (float)rotation, 0.0f, Space.Self);
                newObj.transform.position = position;
                newObj.AddComponent<BoxCollider>();
                BoxCollider box = newObj.GetComponent<BoxCollider>();
                box.size = new Vector3(5.5f, 1f, 7.8f);
                box.center = new Vector3(0f, 0f, -3.9f);
                newObj.AddComponent<MeshCollider>();
            }
            return true;
        }
    };
}