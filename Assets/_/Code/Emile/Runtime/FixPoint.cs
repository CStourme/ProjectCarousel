using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace FixPoint.Runtime
{
    public class FixPoint : MonoBehaviour
    {
        #region Public
        
        [Header ("RayCast Configuration")]
        [SerializeField] private float distance = 20f;
        [SerializeField] private LayerMask m_touchableTarget; 
        [SerializeField] private QueryTriggerInteraction m_TriggerInteraction; 
        
        [Header("UI")]
        public Text timerText;
        public Text foundText;

        [Header("Effets")]
        [SerializeField] private ParticleSystem smokeEffect;

        [Header("Timer")]
        [SerializeField] private float timerDuration = 60f;

        #endregion
        
        float currentTime;
        bool isCounting = true;

        #region Unity API

        public void Start()
        {
            currentTime = timerDuration;
            foundText.gameObject.SetActive(false);
        }

        public void Update()
        {
            HandleTimer();

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance, m_touchableTarget, m_TriggerInteraction))
            {
                Debug.Log("Objet détecté : " + hit.collider.name);

                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Symbole trouvé : " + hit.collider.name);

                    TriggerEffect(hit);
                }
            }
            else
            {
                Debug.Log("Aucun objet détecté");
            }
        }
        
        #endregion
        
        #region Main Methods
        
        void HandleTimer()
        {
            if (!isCounting) return;

            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCounting = false;
                Debug.Log("Elapsed Time !");
            }

            timerText.text = "Time : " + Mathf.Ceil(currentTime).ToString();
        }

        public void TriggerEffect(RaycastHit hit)
        {
            float distanceToObject = Vector3.Distance(transform.position, hit.collider.transform.position);

            if (distanceToObject < 3f)
            {
                if (!smokeEffect.isPlaying)
                    smokeEffect.Play();
            }
            else
            {
                if (smokeEffect.isPlaying)
                    smokeEffect.Stop();
            }

            StartCoroutine(ShowFoundText());
        }

        IEnumerator ShowFoundText()
        {
            foundText.gameObject.SetActive(true);

            Vector3 startPos = foundText.transform.position;
            Vector3 endPos = startPos + new Vector3(0, 50, 0);

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                foundText.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            foundText.gameObject.SetActive(false);
            foundText.transform.position = startPos;
        }

        #endregion
    }
}