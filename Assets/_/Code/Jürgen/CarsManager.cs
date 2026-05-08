using UnityEngine;
using UnityEngine.UI;

public class CarsManager : MonoBehaviour
{
    #region Publics

    public RandomSpawn[] m_spawners;

    public Image m_image;

    #endregion


    #region Unity API

    private void Update()
    {
        CheckItems();
    }

    #endregion


    #region Methods

    void CheckItems()
    {
        bool hasActiveItem = false;

        foreach (RandomSpawn spawner in m_spawners)
        {
            if (spawner.IsItemActive())
            {
                hasActiveItem = true;
                break;
            }
        }

        // Si plus aucun objet actif -> image visible
        m_image.enabled = !hasActiveItem;
    }

    #endregion
}