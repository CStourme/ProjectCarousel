using System;
using Thomas.Runtime;
using UnityEngine;

namespace Runtime.Bunyamin
{
    public class TestSymbol : MonoBehaviour
    {
        
        
        #region Unity API

        private void Start()
        {
            _charWord = FindFirstObjectByType<CharWord>();
            ControlCar();
        }

        #endregion
        
        
        #region Utils

        private void ControlCar()
        {
            m_found = FindObjectsByType<ValidationSymbol>(FindObjectsSortMode.None);

            for (int i = 0; i < m_found.Length; i++)
            {
                m_found[i].SetFound(false);
            }
            
            _charWord.RandomizerLetterDisplay();
        }
        
        #endregion
        
        
        #region  Private

        private ValidationSymbol[] m_found;
        private CharWord _charWord;

        #endregion
    }
}
