using FixPoint.Runtime;
using Thomas.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomSpawn : MonoBehaviour
{
    #region Publics

    public GameObject itemPrefab;
    public Transform[] spawnPoints;

    public Image m_image;

    public CharWord m_charWord;

    #endregion


    #region Unity API

    private void Awake()
    {
        _raycastTimer.OnScanComplete += HandleScanComplete;

        SpawnItem();
    }

    private void Update()
    {
        CheckItemState();
    }

    #endregion


    #region Main Methods

    private void SpawnItem()
    {
        if (spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);

        _currentItem = Instantiate(
            itemPrefab,
            spawnPoints[randomIndex].position,
            spawnPoints[randomIndex].rotation,
            spawnPoints[randomIndex]
        );

        _wasActive = _currentItem.activeInHierarchy;
    }

    private void HandleScanComplete(GameObject scannedObject)
    {
        // Ignore les autres objets
        if (scannedObject != _currentItem) return;

        FoundItem();
    }

    public bool IsItemActive()
    {
        if (_itemFound) return false;

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

        Debug.Log("Correct object found !");
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

    #endregion


    #region Privates

    private GameObject _currentItem;

    private bool _itemFound;
    private bool _wasActive;

    [SerializeField] private RaycastTimer _raycastTimer = null;

    #endregion
}