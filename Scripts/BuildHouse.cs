using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BuildHouse : MonoBehaviour
{
    class HouseGenerator{
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
            new HouseInfo(15, 0.1f, 0f),
            new HouseInfo(-1, 0f, 0f)
            
        };

        private HouseInfo[] center = new[]{
            new HouseInfo(23, 0.4f, -0.6f),
            new HouseInfo(15, 0.1f, 0f),
            new HouseInfo(-1, 0f, 0f)
        };

        private const int TOTALADDONS = 4;

        public HouseGenerator(){
            prefabs = Resources.LoadAll<GameObject>("Buildings/Prefabs");
            city = new GameObject("All Buildings");
            city.tag = "House";
            houses = 0;
        }

        public bool GenerateHouse(int x, int z, int rotation, int maxwidth, int maxdepth){
            if (maxwidth <= 5 || maxdepth <= 7) return false;

            houses++;
            GameObject newObj = new GameObject("House" + houses.ToString());
            newObj.transform.SetParent(city.transform);
            newObj.tag = "House";
            
            int type = Random.Range(0, 0);

            if(type == 0){
                GameObject addon;
                int rand;
                //Build base
                GameObject base1child = Instantiate(prefabs[0], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                GameObject base2child = Instantiate(prefabs[0], new Vector3(0,0,0), Quaternion.identity) as GameObject;

                base1child.transform.SetParent(newObj.transform);
                base1child.transform.position = new Vector3(0f, 0f, -2.11f);
                base1child.name = "House" + houses.ToString() + " - Main_Building1";
                base2child.transform.SetParent(newObj.transform);
                base2child.transform.position = new Vector3(0f, 0f, -5.53f);
                base2child.transform.Rotate(0f, 180.0f, 0.0f, Space.Self);
                base2child.name = "House" + houses.ToString() + " - Main_Building2";

                //Build addons
                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (edge[rand].id != -1){
                    addon = Instantiate(prefabs[edge[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(2.6f + edge[rand].depthdiff, 2.1f + edge[rand].heightdiff, -6.51f);
                    addon.transform.Rotate(0f, 90.0f, 0.0f, Space.Self);
                }
                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (edge[rand].id != -1){
                    addon = Instantiate(prefabs[edge[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(2.6f + edge[rand].depthdiff, 2.1f + edge[rand].heightdiff, -1.21f);
                    addon.transform.Rotate(0f, 90.0f, 0.0f, Space.Self);
                }
                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (center[rand].id != -1){
                    addon = Instantiate(prefabs[center[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(2.6f + center[rand].depthdiff, 2.1f + center[rand].heightdiff, -3.74f);
                    addon.transform.Rotate(0f, 90.0f, 0.0f, Space.Self);
                }

                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (edge[rand].id != -1){
                    addon = Instantiate(prefabs[edge[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(-2.6f - edge[rand].depthdiff, 2.1f + edge[rand].heightdiff, -6.51f);
                    addon.transform.Rotate(0f, 270.0f, 0.0f, Space.Self);
                }
                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (edge[rand].id != -1){
                    addon = Instantiate(prefabs[edge[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(-2.6f - edge[rand].depthdiff, 2.1f + edge[rand].heightdiff, -1.21f);
                    addon.transform.Rotate(0f, 270.0f, 0.0f, Space.Self);
                }
                rand = Random.Range(0, TOTALADDONS);
                Debug.Log(rand);
                if (center[rand].id != -1){
                    addon = Instantiate(prefabs[center[rand].id], new Vector3(0,0,0), Quaternion.identity) as GameObject;
                    addon.transform.SetParent(newObj.transform);
                    addon.transform.position = new Vector3(-2.6f - center[rand].depthdiff, 2.1f + center[rand].heightdiff, -3.74f);
                    addon.transform.Rotate(0f, 270.0f, 0.0f, Space.Self);
                }
            }
            newObj.transform.Rotate(0f, (float)rotation, 0.0f, Space.Self);
            newObj.transform.position = new Vector3((float)x, 0f, (float)z);

            return true;
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        // HouseGenerator housegenerator = new HouseGenerator();
        // housegenerator.GenerateHouse(20, 20, 0, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 45, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 90, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 135, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 180, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 225, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 270, 10, 10);
        // housegenerator.GenerateHouse(20, 20, 315, 10, 10);

        GameObject newObj = new GameObject("test");
        Instantiate(newObj, Vector3.zero, Quaternion.identity);
        GameObject[] myObjects = Resources.LoadAll<GameObject>("Buildings/Prefabs");

        for (int i = 0; i < 189; i++){
            GameObject base1child = Instantiate(myObjects[i], new Vector3(0,0,0), Quaternion.identity) as GameObject;
            base1child.transform.SetParent(newObj.transform);
            base1child.transform.position = new Vector3(0f + (float)i * 15f, 0f, 0f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}