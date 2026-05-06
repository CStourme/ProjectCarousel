using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraController.Runtime
{
    public class CameraController : MonoBehaviour
    {
        #region Paramètres Exposés

        [Header("Cible & Caméras")]
        [Tooltip("L'objet que les caméras vont fixer du regard.")]
        [SerializeField] private Transform _lookAtTarget;
        [Tooltip("Le pivot parent de la caméra manuelle.")]
        [SerializeField] private GameObject _manualPivot; 
        [Tooltip("L'objet Caméra utilisé pour l'auto-orbit.")]
        [SerializeField] private GameObject _autoCamera;   

        [Header("Centres d'Orbite (Fixes)")]
        [Tooltip("Point central du pivot manuel.")]
        [SerializeField] private Vector3 _manualOrbitCenter = Vector3.zero;
        [Tooltip("Point central du cercle automatique.")]
        [SerializeField] private Vector3 _autoOrbitCenter = Vector3.zero;

        [Header("Réglages Manuel (Clic Gauche)")]
        public float manualRotationSpeed = 0.2f;
        public float minVerticalAngle = -20f;
        public float maxVerticalAngle = 80f;

        [Header("Réglages Auto (Espace)")]
        [SerializeField] private bool _isAutoActive = false;
        public float autoRadius = 7f;
        public float autoHeight = 3f;
        public float autoSpeed = 0.5f;

        [Header("Réglages Zoom (Clic Droit)")]
        public float zoomSpeed = 0.5f;
        public float minFOV = 15f;
        public float maxFOV = 90f;
        public float currentFOV = 60f;

        #endregion

        #region Variables Internes Privées

        private float _verticalRotation = 0f;
        private float _autoAngle = 0f;
        
        // Je stocke les composants ici pour éviter de les chercher à chaque frame (plus performant)
        private Camera _manualCamComponent;
        private Camera _autoCamComponent;

        #endregion

        #region Unity API

        private void Start()
        {
            // Au démarrage, je vais chercher les composants Camera sur mes GameObjects
            if (_manualPivot != null) _manualCamComponent = _manualPivot.GetComponentInChildren<Camera>();
            if (_autoCamera != null) _autoCamComponent = _autoCamera.GetComponent<Camera>();

            // J'applique les réglages de départ
            ApplyCameraSwitch();
            ApplyFOV();
            
            // Je force mon pivot manuel à se placer sur son centre défini
            if (_manualPivot != null) _manualPivot.transform.position = _manualOrbitCenter;
        }

        private void Update()
        {
            // 1. Je gère le zoom (Clic droit)
            HandleZoom();

            // 2. Je gère le changement de mode (Barre Espace)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _isAutoActive = !_isAutoActive;
                ApplyCameraSwitch();
            }

            // 3. J'exécute la logique de mouvement selon le mode choisi
            if (_isAutoActive) RunAutoLogic();
            else RunManualLogic();
        }

        #endregion

        #region Logique de Zoom

        private void HandleZoom()
        {
            // Si je maintiens le clic droit, je modifie le champ de vision (FOV)
            if (Mouse.current.rightButton.isPressed)
            {
                float mouseInputY = Mouse.current.delta.ReadValue().y;
                if (mouseInputY != 0)
                {
                    // Je calcule : monter la souris = zoomer (donc baisser le FOV)
                    currentFOV -= mouseInputY * zoomSpeed;
                    // Je bloque entre mes limites pour ne pas avoir une image de l'espace
                    currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
                    
                    // J'applique le nouveau FOV aux deux caméras pour qu'elles restent synchronisées
                    ApplyFOV();
                }
            }
        }

        private void ApplyFOV()
        {
            // Je vérifie toujours si la caméra existe avant de lui parler
            if (_manualCamComponent != null) _manualCamComponent.fieldOfView = currentFOV;
            if (_autoCamComponent != null) _autoCamComponent.fieldOfView = currentFOV;
        }

        #endregion

        #region Logique des Caméras

        private void RunManualLogic()
        {
            // Je m'assure que mon pivot reste bien au centre de l'orbite
            _manualPivot.transform.position = _manualOrbitCenter;

            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                
                // Rotation gauche/droite (autour de l'axe vertical du monde)
                _manualPivot.transform.Rotate(Vector3.up, delta.x * manualRotationSpeed, Space.World);

                // Rotation haut/bas (je stocke la valeur pour la brider)
                _verticalRotation -= delta.y * manualRotationSpeed;
                _verticalRotation = Mathf.Clamp(_verticalRotation, minVerticalAngle, maxVerticalAngle);

                // J'applique la rotation finale au pivot (X=Vertical, Y=Horizontal actuel)
                float currentY = _manualPivot.transform.localEulerAngles.y;
                _manualPivot.transform.localRotation = Quaternion.Euler(_verticalRotation, currentY, 0);
            }

            // Si j'ai une cible, je force la caméra enfant à la regarder
            if (_lookAtTarget != null && _manualCamComponent != null)
            {
                _manualCamComponent.transform.LookAt(_lookAtTarget.position);
            }
        }

        private void RunAutoLogic()
        {
            // Je fais progresser mon angle au fil du temps
            _autoAngle += autoSpeed * Time.deltaTime;

            // Calcul du cercle (X et Z) autour du centre auto
            float x = Mathf.Cos(_autoAngle) * autoRadius;
            float z = Mathf.Sin(_autoAngle) * autoRadius;

            // Je déplace physiquement la caméra sur son orbite
            _autoCamera.transform.position = new Vector3(
                _autoOrbitCenter.x + x,
                _autoOrbitCenter.y + autoHeight,
                _autoOrbitCenter.z + z
            );

            // Je force la caméra auto à ne pas lâcher la cible des yeux
            if (_lookAtTarget != null)
            {
                _autoCamera.transform.LookAt(_lookAtTarget.position);
            }
        }

        #endregion

        #region Helpers & Gizmos

        private void ApplyCameraSwitch()
        {
            // J'allume la bonne caméra et j'éteins l'autre selon le mode
            if (_autoCamera != null) _autoCamera.SetActive(_isAutoActive);
            if (_manualPivot != null) _manualPivot.SetActive(!_isAutoActive);
        }

        /// <summary>
        /// Cette partie n'est visible que dans l'éditeur. C'est mon radar de débug.
        /// </summary>
        private void OnDrawGizmos()
        {
            // --- 1. LA CIBLE ---
            if (_lookAtTarget != null)
            {
                // Je dessine une sphère magenta sur l'objet cible pour le repérer
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_lookAtTarget.position, 0.3f);

                // --- 2. LES TRAITS DE VISÉE (LA NOUVEAUTÉ) ---
                // Je trace une ligne entre chaque caméra et la cible
                // C'est super pour vérifier si le LookAt fonctionne bien même de loin !
                
                Gizmos.color = Color.white; // Blanc pour que ce soit bien lisible

                // Ligne pour la caméra manuelle (si elle existe)
                if (_manualCamComponent != null)
                {
                    Gizmos.DrawLine(_manualCamComponent.transform.position, _lookAtTarget.position);
                }

                // Ligne pour la caméra auto (si elle existe)
                if (_autoCamComponent != null)
                {
                    Gizmos.DrawLine(_autoCamComponent.transform.position, _lookAtTarget.position);
                }
            }

            // --- 3. LES CENTRES D'ORBITES ---
            Gizmos.color = Color.yellow;
            // Cube pour le manuel, Sphère pour l'auto
            Gizmos.DrawWireCube(_manualOrbitCenter, Vector3.one * 0.5f);
            Gizmos.DrawWireSphere(_autoOrbitCenter, 0.5f);

            // --- 4. LE RAIL DE LA CAMÉRA AUTO ---
            Gizmos.color = Color.cyan;
            Vector3 lastPoint = Vector3.zero;
            for (int i = 0; i <= 64; i++)
            {
                float stepAngle = i * Mathf.PI * 2 / 64;
                Vector3 pos = new Vector3(Mathf.Cos(stepAngle) * autoRadius, autoHeight, Mathf.Sin(stepAngle) * autoRadius) + _autoOrbitCenter;
                if (i > 0) Gizmos.DrawLine(lastPoint, pos);
                lastPoint = pos;
            }
        }

        #endregion
    }
}