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
        
        
        
        #region Main Methods

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ToggleMute()
        {
            Mute = !Mute;
            m_Music.mute = Mute;
        }

    #endregion 
        
        
        #region Privates
        
        private bool Mute = false;
        [SerializeField] private AudioSource m_Music;   
        
        #endregion
    }
}
