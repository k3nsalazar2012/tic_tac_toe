using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        [SerializeField]    List<Pool> poolList;
        public Dictionary<string, Queue<GameObject>> poolDictionary;
        private List<GameObject> objectsInScene;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            objectsInScene = new List<GameObject>();

            foreach (Pool pool in poolList)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i =0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.transform.SetParent(transform, true);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public void ResetPool()
        {
            foreach (GameObject obj in objectsInScene)
            {
                obj.transform.SetParent(transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localEulerAngles = Vector3.zero;
                obj.SetActive(false);
            }

            objectsInScene.Clear();
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Transform parent)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.SetParent(parent);
            //objectToSpawn.transform.position = position;
            objectToSpawn.transform.localPosition = Vector3.zero;
            objectToSpawn.transform.localScale = Vector3.one;
            objectToSpawn.transform.localEulerAngles = Vector3.zero;
            poolDictionary[tag].Enqueue(objectToSpawn);

            objectsInScene.Add(objectToSpawn);

            return objectToSpawn;
        }
    }
}