using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Thomas.Runtime
{
    public class CharWord : MonoBehaviour
    {
        #region Publics

        public string[] m_word;

        [Header("References")]
        public Transform m_parent;
        public TMP_Text m_letterPrefab;

        #endregion


        #region Unity API

        private void Awake()
        {
            PickRandomWord();
            CreateLetters();
        }

        #endregion


        #region Main Methods

        public void RandomizerLetterDisplay()
        {
            if (_hiddenLetters.Count == 0)
            {
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
            foreach (char c in _selectedWord)
            {
                TMP_Text letter = Instantiate(m_letterPrefab, m_parent);

                letter.text = c.ToString();
                letter.gameObject.SetActive(false);

                _hiddenLetters.Add(letter);
            }
        }

        private void PickRandomWord()
        { 
            if (m_word == null || m_word.Length == 0 ) return;
                
            int index = Random.Range(0, m_word.Length);
            _selectedWord = m_word[index];
        }

        #endregion


        #region Private and Protected

        private readonly List<TMP_Text> _hiddenLetters = new();
        private string _selectedWord;

        #endregion
    }
}