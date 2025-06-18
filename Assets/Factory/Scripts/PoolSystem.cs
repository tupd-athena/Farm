using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Factory
{
    [System.Serializable]
    public class PoolItemData
    {
        public string id;
        public GameObject prefab;
        public int amount;
    }

    public class PoolSystem : MonoBehaviour
    {
        public static PoolSystem Instance;
        [SerializeField]
        private List<PoolItemData> poolItemData = new List<PoolItemData>();
        private Dictionary<string, List<GameObject>> _pooledObjects =
            new Dictionary<string, List<GameObject>>();

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializeAllPools();
        }

        private void InitializeAllPools()
        {
            foreach (var itemData in poolItemData)
            {
                InitializePool(itemData);
            }
        }

        private void InitializePool(PoolItemData itemData)
        {
            if (!_pooledObjects.ContainsKey(itemData.id))
            {
                _pooledObjects[itemData.id] = new List<GameObject>();
            }

            // Create initial pool of objects
            for (int i = 0; i < itemData.amount; i++)
            {
                CreateNewObject(itemData);
            }
        }

        private GameObject CreateNewObject(PoolItemData itemData)
        {
            GameObject obj = Instantiate(itemData.prefab, transform);
            obj.SetActive(false);
            _pooledObjects[itemData.id].Add(obj);
            return obj;
        }

        public GameObject GetObject(string id)
        {
            if (!_pooledObjects.ContainsKey(id))
            {
                Debug.Log($"<color=red>Pool with ID {id} does not exist!<>/color>");
                return null;
            }

            var pool = _pooledObjects[id];
            var poolActive = pool.ToList().FindAll(x => !x.activeSelf);
            var itemData = poolItemData.Find(x => x.id == id);

            GameObject obj = poolActive.FirstOrDefault();
            if (obj == null && itemData != null)
            {
                obj = CreateNewObject(itemData);
            }

            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj, string id)
        {
            if (!_pooledObjects.ContainsKey(id))
            {
                Debug.Log($"<color=red>Pool with ID {id} does not exist!<>/color>");
                return;
            }
            if (obj == null)
            {
                Debug.Log("<color=red>Object to return is null!</color>");
                return;
            }
            obj.transform.SetParent(transform);
            obj.SetActive(false);
        }

        public void ReturnObject(GameObject obj, string id, float delay)
        {
            if (!_pooledObjects.ContainsKey(id))
            {
                Debug.Log($"<color=red>Pool with ID {id} does not exist!<>/color>");
                return;
            }
            if (obj == null)
            {
                Debug.Log("<color=red>Object to return is null!</color>");
                return;
            }
            StartCoroutine(ReturnObjectCoroutine(obj, id, delay));
        }

        private IEnumerator ReturnObjectCoroutine(GameObject obj, string id, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
        }

        public void ClearPool(string id)
        {
            if (!_pooledObjects.ContainsKey(id))
            {
                Debug.Log($"<color=red>Pool with ID {id} does not exist!<>/color>");
                return;
            }
            var pool = _pooledObjects[id];
            while (pool.Count > 0)
            {
                GameObject obj = pool.FirstOrDefault();
                Destroy(obj);
            }
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pooledObjects.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.FirstOrDefault();
                    Destroy(obj);
                }
            }
            _pooledObjects.Clear();
        }
    }
}
