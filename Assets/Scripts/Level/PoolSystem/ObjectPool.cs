using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

    //public static ObjectPool Instance { get; private set; }

    [field: Header("Pool System Properties")]
    // El objeto padre de todos los diccionarios
    [field: SerializeField] private Transform poolParent { get; set; }
    [field: SerializeField, ReadOnlyField] private Dictionary<string, Queue<GameObject>> poolDictionary { get; set; } = new();
    // Los padres de cada diccionario
    [field: SerializeField, ReadOnlyField] private Dictionary<string, Transform> poolParents { get; set; } = new();



    /*void Awake() {
        poolDictionary = new();
        poolParents = new();
    }*/

    public GameObject SpawnFromPool(string tag, GameObject prefab, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(tag)) {
            // Si el diccionario no existe, crea uno nuevo y también crea un nuevo objeto padre para él
            poolDictionary[tag] = new Queue<GameObject>();

            GameObject newParent = new GameObject(tag);

            newParent.transform.parent = poolParent;
            poolParents[tag] = newParent.transform;
        }

        if (poolDictionary[tag].Count == 0) {
            // Si no hay objetos disponibles, instancia uno nuevo y lo hace hijo del objeto padre correspondiente
            GameObject newObj = Instantiate(prefab, position, rotation, poolParents[tag]);
            //newObj.SetActive(false);
            poolDictionary[tag].Enqueue(newObj);

            newObj = poolDictionary[tag].Dequeue();

            return newObj;
        }

        // Obtiene un objeto del pool
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();   

        // Activa el objeto y lo coloca en la posición deseada
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn) {
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
