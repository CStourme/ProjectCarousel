using UnityEngine;

public class CarSwitch : MonoBehaviour
{
    #region Publics
    public GameObject[] m_ListeVoitures;
    
    #endregion

    #region Unity API

    void Start()
    {
        ActualiserAffichage();
    }

    void Update()
    {
     
    }

    void ChangerVoiture(int direction)
    {
        _Index += direction;

        if (_Index > 3)
        {
            _Index = 0;
        }
        else if (_Index < 0)
        {
            _Index = 3;
        }

        ActualiserAffichage();
    }

    #endregion

    #region Main Methods

    void ActualiserAffichage()
    {
        foreach (GameObject voiture in m_ListeVoitures)
        {
            voiture.SetActive(false);
        }


        m_ListeVoitures[_Index].SetActive(true);

        Debug.Log("Voiture actuelle : " + (_Index + 1));
    }

    public void BoutonSuivant()
    {
        ChangerVoiture(1);
    }

    public void BoutonPrecedent()
    {
        ChangerVoiture(-1);
    }

    #endregion

    #region Utils



    #endregion

    #region Private
    private int _Index = 0;


    #endregion
}
