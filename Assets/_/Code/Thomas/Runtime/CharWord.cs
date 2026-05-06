using TMPro;
using UnityEngine;

namespace Thomas.Runtime
{
    public class CharWord : MonoBehaviour
    {
        #region Publics

        public string m_word;
        public TMP_Text[] m_text;

        #endregion
        
        #region Unity API

        private void Start()
        {
            CharText();
        }
        
        #endregion
        
        
        #region Utils

        private void CharText()
        {
            int length = Mathf.Min(m_word.Length, m_text.Length);

            for (int i = 0; i < length; i++)
            {
                m_text[i].text = m_word[i].ToString();
            }
        }
        
        #endregion
        
        
        #region Privates and Protected

        

        #endregion
    }
}
