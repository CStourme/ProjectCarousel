using UnityEngine;
using UnityEngine.EventSystems; // J'ajoute ça pour parler au système d'événements

namespace CameraController.Runtime
{
    // Je force l'objet à avoir un Collider, sinon ce script ne servira à rien !
    [RequireComponent(typeof(Collider))]
    public class FocusPoint : MonoBehaviour
    {
        private CameraController _manager;

        private void Start()
        {
            // "Ok, je cherche mon patron (le manager) dans la scène."
            _manager = Object.FindFirstObjectByType<CameraController>();

            if (_manager == null)
            {
                Debug.LogError("ERREUR : FocusPoint sur " + gameObject.name + " ne voit pas le CameraController !");
            }
        }

        // AU LIEU DE OnMouseDown, j'utilise une détection manuelle plus fiable
        private void Update()
        {
            // "À chaque image, je regarde si l'utilisateur a cliqué sur le bouton GAUCHE."
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                // "Si oui, je lance un rayon depuis la souris vers le monde 3D."
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                RaycastHit hit;

                // "Si ce rayon me touche MOI (mon collider)..."
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        // "C'est la fête ! J'appelle le manager."
                        TriggerFocus();
                    }
                }
            }
        }

        private void TriggerFocus()
        {
            if (_manager != null)
            {
                Debug.Log("--- CLIC DÉTECTÉ sur : " + gameObject.name + " ---");
                
                // J'envoie ma position pour le centre de rotation et mon transform pour le regard
                _manager.UpdateCameraFocus(transform.position, transform);
            }
        }
    }
}