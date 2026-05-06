using UnityEngine;

namespace Runtime.Bunyamin
{
    public class TestSymbol : MonoBehaviour
    {
        
        
        #region Unity API
        
        
        
        #endregion
        
        
        #region Utils

        private void ControlCar()
        {
            for (int i = 0; i < m_objects.Length; i++)
            {
                if (m_objects[i])
                {
                    m_objects[i].GetComponentInChildren<ValidationSymbol>().SetFound(false);
                }
                
            }
        }
        
        #endregion
        
        
        #region  Private

        private ValidationSymbol m_found;
        [SerializeField] private GameObject[]  m_objects;

        #endregion
    }
}
