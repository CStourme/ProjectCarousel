using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform[] spawnPoints;

    public Image m_image;

    void Awake()
    {
        if (!_itemFound)
        {
            SpawnItem();
        }
    }
    
    void SpawnItem()
    {
        if (spawnPoints.Length == 0) return;
        Debug.Log("TEST");
     
        int randomIndex = Random.Range(0, spawnPoints.Length);
        
        _currentItem = Instantiate(itemPrefab, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation, spawnPoints[randomIndex]);

    }

    public bool IsItemActive()
    {
        if(_itemFound) return false;
        
        if (!_currentItem) return false;

        return _currentItem.activeInHierarchy;
    }

    public void FoundItem()
    {
        _itemFound = true;

        if (_currentItem)
        {
            _currentItem.SetActive(false);
        }
    }
    
    private GameObject _currentItem;
    private bool _itemFound;
    
}
