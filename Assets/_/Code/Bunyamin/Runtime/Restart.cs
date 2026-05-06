using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.Bunyamin
{
    public class Restart : MonoBehaviour
    {
        #region Publics
        
        
        #endregion
        
        #region Unity API
        
        #endregion
        
        
        #region Privates
        
        #endregion
 
        #region Main Methods

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        #endregion
    }
}
