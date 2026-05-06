using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FixPoint.Runtime
{
    public class FixPoint : MonoBehaviour
    {
        #region Public
        
        [Header ("Configuration du Raycast (Laser de détection)")]
        
        public float distance = 20f;


        public LayerMask m_touchableTarget; 

        // Permet de choisir si le Raycast touche les triggers
        public QueryTriggerInteraction m_TriggerInteraction; 
        
        #endregion
        
        #region Unity API

        public void Update()
        {
            // origine = position de la caméra
            // direction = là où la caméra regarde
            Ray ray = new Ray(transform.position, transform.forward);

            // Variable qui va contenir les infos de l’objet touché
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, distance, m_touchableTarget, m_TriggerInteraction))
            {
                Debug.Log("Objet détecté : " + hit.collider.name);

                if (hit.collider.gameObject.layer == m_touchableTarget)
                {
                    Debug.Log("Symbole trouvé : " + hit.collider.name);

                }
                
            }
            else
            {
                Debug.Log("Aucun objet détecté");
            }
        }
        
        #endregion
        
        #region Main Methods
        
        
        #endregion
    }
}