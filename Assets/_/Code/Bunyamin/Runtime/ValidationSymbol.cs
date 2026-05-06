using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Bunyamin
{
    public class ValidationSymbol : MonoBehaviour
    {
        #region Publics
        
        public Image m_found;
        
        #endregion

        #region Privates

        #endregion

        #region Unity API

        private void Awake()
        {
            m_found.enabled = false;
        }

        private void OnEnable()
        {
            m_found.enabled = false;
        }

        private void OnDisable()
        {
            m_found.enabled = true;
        }

        #endregion
        
        #region Main Methods

        [ContextMenu("Set Found")]
        public void SetFound(bool found)
        {
            m_found.enabled = found;
        }

        #endregion

    }
}
