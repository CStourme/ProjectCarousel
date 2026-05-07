using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        private void Awake()
        {
            CreateLetters();
        }

        #endregion


        #region Main Methods

        public void RandomizerLetterDisplay()
        {
            if (_hiddenLetters.Count == 0)
            {
                Debug.Log("Toutes les lettres sont affichées.");
                return;
            }

            int randomIndex = Random.Range(0, _hiddenLetters.Count);

            TMP_Text randomLetter = _hiddenLetters[randomIndex];

            randomLetter.gameObject.SetActive(true);

            _hiddenLetters.RemoveAt(randomIndex);
        }

        #endregion


        #region Utils

        private void CreateLetters()
        {
            foreach (char c in m_word)
            {
                TMP_Text letter = Instantiate(m_letterPrefab, m_parent);

                letter.text = c.ToString();
                letter.gameObject.SetActive(false);

                _hiddenLetters.Add(letter);
            }
        }

        #endregion


        #region Private and Protected

        private readonly List<TMP_Text> _hiddenLetters = new();

        #endregion
    }
}