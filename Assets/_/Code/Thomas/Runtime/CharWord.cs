using TMPro;
using UnityEngine;

namespace Thomas.Runtime
{
    public class CharWord : MonoBehaviour
    {
        #region Publics

        public string m_word;

        [Header("References")]
        public Transform m_parent;
        public TMP_Text m_letterPrefab;

        #endregion


        #region Unity API

        private void Start()
        {
            CreateLetters();
        }

        #endregion


        #region Utils

        private void CreateLetters()
        {
            foreach (char c in m_word)
            {
                TMP_Text letter = Instantiate(m_letterPrefab, m_parent);

                letter.text = c.ToString();
            }
        }

        #endregion
    }
}