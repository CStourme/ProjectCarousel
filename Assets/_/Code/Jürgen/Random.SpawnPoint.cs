using System;
using Thomas.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform[] spawnPoints;

    public Image m_image;
    
    public CharWord m_charWord;

    void Awake()
    {
        if (!_itemFound)
        {
            SpawnItem();
        }
    }

    private void Update()
    {
        CheckItemState();
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
        if (_itemFound) return;
        
        _itemFound = true;

        if (_currentItem)
        {
            _currentItem.SetActive(false);
        }

        if (m_charWord)
        {
            m_charWord.RandomizerLetterDisplay();
        }
    }

    private void CheckItemState()
    {
        if (!_currentItem || _itemFound) return;
        
        bool isActive = _currentItem.activeInHierarchy;

        if (_wasActive && !isActive)
        {
            FoundItem();
        }
        
        _wasActive = isActive;
    }
    
    private GameObject _currentItem;
    private bool _itemFound;
    private bool _wasActive;
}
