using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform[] spawnPoints;

    void Awake()
    {
        SpawnItem();
    }

    void SpawnItem()
    {
        if (spawnPoints.Length == 0) return;
        Debug.Log("TEST");
     
        int randomIndex = Random.Range(0, spawnPoints.Length);

        itemPrefab.SetActive(true);
        Instantiate(itemPrefab, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation, spawnPoints[randomIndex]);

    }
}
