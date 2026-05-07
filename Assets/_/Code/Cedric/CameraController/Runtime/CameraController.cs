using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraController.Runtime
{
    public class CameraController : MonoBehaviour
    {
        #region Paramètres Exposés

        [Header("Cible & Caméras")]
        [SerializeField] private Transform _lookAtTarget;
        [SerializeField] private GameObject _manualPivot; 
        [SerializeField] private GameObject _autoCamera;   

        [Header("Centres d'Orbite (Fixes au départ)")]
        [SerializeField] private Vector3 _manualOrbitCenter = Vector3.zero;
        [SerializeField] private Vector3 _autoOrbitCenter = Vector3.zero;

        // --- NOUVEAU : RÉGLAGES DE TRANSITION ---
        [Header("Réglages de Transition (Lerp)")]
        [Tooltip("Vitesse de transition du regard (0.1 = lent, 10 = rapide).")]
        public float lookAtLerpSpeed = 5f;
        [Tooltip("Vitesse de transition du centre de l'orbite (0.1 = lent, 10 = rapide).")]
        public float centerLerpSpeed = 5f;

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
        private Camera _manualCamComponent;
        private Camera _autoCamComponent;

        // --- NOUVEAU : VARIABLES DE CALCUL POUR LE LERP ---
        // Je crée des variables "tampons" pour stocker là où je veux aller
        private Vector3 _targetOrbitPosition; 
        // Je stocke le point précis que la caméra regarde actuellement pour le faire glisser
        private Vector3 _currentLookAtPos;

        #endregion

        #region Unity API

        private void Start()
        {
            if (_manualPivot != null) _manualCamComponent = _manualPivot.GetComponentInChildren<Camera>();
            if (_autoCamera != null) _autoCamComponent = _autoCamera.GetComponent<Camera>();

            ApplyCameraSwitch();
            ApplyFOV();
            
            // --- INITIALISATION DES CIBLES ---
            // Au début, la cible de mouvement est la position de départ définie dans l'inspecteur
            _targetOrbitPosition = _manualOrbitCenter;

            // Et le regard commence pile sur la cible pour éviter un glissement bizarre au lancement
            if (_lookAtTarget != null) _currentLookAtPos = _lookAtTarget.position;

            if (_manualPivot != null) _manualPivot.transform.position = _manualOrbitCenter;
        }

        private void Update()
        {
            // --- MISE À JOUR DES LERPS (MOUVEMENT DOUX) ---
            // 1. Je fais glisser le centre de mes orbites vers la cible de destination
            // "Je me dis : peu importe où je suis, je me rapproche de _targetOrbitPosition petit à petit"
            _manualOrbitCenter = Vector3.Lerp(_manualOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);
            _autoOrbitCenter = Vector3.Lerp(_autoOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);

            // 2. Je fais glisser le point de regard vers la position réelle de la cible
            // "Même si la cible bouge (ex: une roue qui tourne), le regard va la suivre avec un léger retard fluide"
            if (_lookAtTarget != null)
            {
                _currentLookAtPos = Vector3.Lerp(_currentLookAtPos, _lookAtTarget.position, Time.deltaTime * lookAtLerpSpeed);
            }

            HandleZoom();

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _isAutoActive = !_isAutoActive;
                ApplyCameraSwitch();
            }

            if (_isAutoActive) RunAutoLogic();
            else RunManualLogic();
        }

        #endregion

        #region Logique de Changement de Focus

        public void UpdateCameraFocus(Vector3 newPoint, Transform newTarget)
        {
            // "Ici, je ne téléporte plus rien ! Je donne juste une nouvelle destination."
            
            // Je change la cible de référence (le script de la roue l'envoie)
            _lookAtTarget = newTarget;

            // Je mets à jour la destination. Le code dans l'Update s'occupera de faire le voyage doucement.
            _targetOrbitPosition = newPoint;
        }

        #endregion
        
        #region Logique de Zoom

        private void HandleZoom()
        {
            if (Mouse.current.rightButton.isPressed)
            {
                float mouseInputY = Mouse.current.delta.ReadValue().y;
                if (mouseInputY != 0)
                {
                    currentFOV -= mouseInputY * zoomSpeed;
                    currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
                    ApplyFOV();
                }
            }
        }

        private void ApplyFOV()
        {
            if (_manualCamComponent != null) _manualCamComponent.fieldOfView = currentFOV;
            if (_autoCamera != null) _autoCamComponent.fieldOfView = currentFOV;
        }

        #endregion

        #region Logique des Caméras

        private void RunManualLogic()
        {
            // J'utilise _manualOrbitCenter qui est maintenant devenu fluide grâce au Lerp dans l'Update
            _manualPivot.transform.position = _manualOrbitCenter;

            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                _manualPivot.transform.Rotate(Vector3.up, delta.x * manualRotationSpeed, Space.World);

                _verticalRotation -= delta.y * manualRotationSpeed;
                _verticalRotation = Mathf.Clamp(_verticalRotation, minVerticalAngle, maxVerticalAngle);

                float currentY = _manualPivot.transform.localEulerAngles.y;
                _manualPivot.transform.localRotation = Quaternion.Euler(_verticalRotation, currentY, 0);
            }

            // "Au lieu de regarder la cible brute, je regarde le point 'tampon' qui glisse"
            if (_manualCamComponent != null)
            {
                _manualCamComponent.transform.LookAt(_currentLookAtPos);
            }
        }

        private void RunAutoLogic()
        {
            _autoAngle += autoSpeed * Time.deltaTime;

            float x = Mathf.Cos(_autoAngle) * autoRadius;
            float z = Mathf.Sin(_autoAngle) * autoRadius;

            // J'utilise _autoOrbitCenter qui est lui aussi devenu fluide
            _autoCamera.transform.position = new Vector3(
                _autoOrbitCenter.x + x,
                _autoOrbitCenter.y + autoHeight,
                _autoOrbitCenter.z + z
            );

            // Idem ici : on fixe le point fluide
            _autoCamera.transform.LookAt(_currentLookAtPos);
        }

        #endregion

        #region Helpers & Gizmos

        private void ApplyCameraSwitch()
        {
            if (_autoCamera != null) _autoCamera.SetActive(_isAutoActive);
            if (_manualPivot != null) _manualPivot.SetActive(!_isAutoActive);
        }

        private void OnDrawGizmos()
        {
            // Je dessine la destination finale en magenta (la cible réelle)
            if (_lookAtTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_lookAtTarget.position, 0.2f);
                
                // Je dessine le point de regard ACTUEL en blanc (celui qui glisse)
                // "C'est pratique pour voir le Lerp travailler en temps réel !"
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(_currentLookAtPos, 0.1f);

                Gizmos.color = Color.white;
                if (_manualCamComponent != null) Gizmos.DrawLine(_manualCamComponent.transform.position, _currentLookAtPos);
                if (_autoCamComponent != null) Gizmos.DrawLine(_autoCamComponent.transform.position, _currentLookAtPos);
            }

            // Les centres jaunes suivent maintenant le mouvement fluide
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_manualOrbitCenter, Vector3.one * 0.5f);
            Gizmos.DrawWireSphere(_autoOrbitCenter, 0.5f);

            Gizmos.color = Color.cyan;
            Vector3 lastPoint = Vector3.zero;
            for (int i = 0; i <= 64; i++)
            {
                float stepAngle = i * Mathf.PI * 2 / 64;
                // Le rail se dessine autour du centre fluide, donc le rail "voyage" aussi !
                Vector3 pos = new Vector3(Mathf.Cos(stepAngle) * autoRadius, autoHeight, Mathf.Sin(stepAngle) * autoRadius) + _autoOrbitCenter;
                if (i > 0) Gizmos.DrawLine(lastPoint, pos);
                lastPoint = pos;
            }
        }

        #endregion
    }
}