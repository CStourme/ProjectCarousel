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

        [Header("Centres d'Orbite")]
        [SerializeField] private Vector3 _manualOrbitCenter = Vector3.zero;
        [SerializeField] private Vector3 _autoOrbitCenter = Vector3.zero;

        [Header("Réglages de Transition (Lerp)")]
        public float lookAtLerpSpeed = 5f;
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
        public float autoOrbitTiltX = 0f; 

        [Header("Réglages Dynamic FOV (Auto uniquement)")]
        [Tooltip("Intensité de base du zoom/dézoom lors d'une impulsion.")]
        public float dynamicFOVAmount = 1.5f;
        [Tooltip("Valeur aléatoire ajoutée à l'intensité de base.")]
        public float dynamicFOVAmountRandomness = 1.0f;
        [Tooltip("Temps de repos minimum entre deux impulsions.")]
        public float dynamicFOVInterval = 3f;
        [Tooltip("Ajout aléatoire au temps de repos pour casser la répétition.")]
        public float dynamicFOVRandomness = 2f;
        [Tooltip("Vitesse de transition du FOV (Aller et Retour).")]
        public float dynamicFOVSmoothness = 2f;

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
        private Vector3 _targetOrbitPosition; 
        private Vector3 _currentLookAtPos;

        private float _dynamicFOVOffset = 0f;       
        private float _targetDynamicOffset = 0f;    
        private float _nextPulseTimer = 0f;         
        #endregion

        #region Unity API
        private void Start()
        {
            if (_manualPivot != null) _manualCamComponent = _manualPivot.GetComponentInChildren<Camera>();
            if (_autoCamera != null) _autoCamComponent = _autoCamera.GetComponent<Camera>();
            
            ApplyCameraSwitch();
            ApplyFOV();
            
            _targetOrbitPosition = _manualOrbitCenter;
            if (_lookAtTarget != null) _currentLookAtPos = _lookAtTarget.position;
            if (_manualPivot != null) _manualPivot.transform.position = _manualOrbitCenter;
            
            ResetPulseTimer();
        }

        private void Update()
        {
            _manualOrbitCenter = Vector3.Lerp(_manualOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);
            _autoOrbitCenter = Vector3.Lerp(_autoOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);

            if (_lookAtTarget != null)
            {
                _currentLookAtPos = Vector3.Lerp(_currentLookAtPos, _lookAtTarget.position, Time.deltaTime * lookAtLerpSpeed);
            }

            if (_isAutoActive) 
            {
                HandleDynamicFOV();
            }
            else 
            {
                // "Si je quitte le mode auto, je ramène doucement le FOV à sa valeur normale."
                _targetDynamicOffset = 0f;
                _dynamicFOVOffset = Mathf.Lerp(_dynamicFOVOffset, 0f, Time.deltaTime * dynamicFOVSmoothness);
                ApplyFOV();
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

        #region Logique Dynamic FOV (Calcul du Pulse Aléatoire)

        private void HandleDynamicFOV()
        {
            // "Je fais défiler mon timer interne."
            _nextPulseTimer -= Time.deltaTime;

            // "Quand le timer expire, c'est le moment de créer une nouvelle variation."
            if (_nextPulseTimer <= 0f)
            {
                // "Étape 1 : Le Signe (Positif ou Négatif)."
                // "Je tire à pile ou face : soit je zoom (négatif), soit je dézoom (positif)."
                // "Random.value renvoie un chiffre entre 0 et 1. Si c'est > 0.5, c'est 1, sinon -1."
                float randomSign = (Random.value > 0.5f) ? 1f : -1f;

                // "Étape 2 : L'Amplitude."
                // "Je calcule une force aléatoire basée sur mes paramètres exposés."
                float randomForce = dynamicFOVAmount + Random.Range(0f, dynamicFOVAmountRandomness);

                // "Étape 3 : La Cible."
                // "Je multiplie ma force par mon signe pour savoir vers où mon Lerp doit se diriger."
                _targetDynamicOffset = randomSign * randomForce;

                // "Étape 4 : Le Reset."
                // "Je relance le chrono pour que l'action ne se répète pas tout de suite."
                ResetPulseTimer();
            }

            // "Étape 5 : Le Retour au Repos."
            // "Si je suis arrivé très près de ma cible (le pic du zoom ou du dézoom),"
            // "je change immédiatement ma cible vers 0 pour entamer le retour fluide."
            if (Mathf.Abs(_dynamicFOVOffset - _targetDynamicOffset) < 0.05f)
            {
                _targetDynamicOffset = 0f;
            }

            // "Étape 6 : L'Application du mouvement."
            // "Je fais glisser ma valeur actuelle vers ma cible avec un Lerp pour que ce soit doux."
            _dynamicFOVOffset = Mathf.Lerp(_dynamicFOVOffset, _targetDynamicOffset, Time.deltaTime * dynamicFOVSmoothness);

            // "J'applique le résultat final aux FOV des caméras."
            ApplyFOV();
        }

        private void ResetPulseTimer()
        {
            // "Je définis un délai aléatoire pour que le rythme de respiration ne soit pas prévisible."
            _nextPulseTimer = dynamicFOVInterval + Random.Range(0f, dynamicFOVRandomness);
        }

        #endregion

        #region Logique de Changement de Focus
        public void UpdateCameraFocus(Vector3 newPoint, Transform newTarget)
        {
            _lookAtTarget = newTarget;
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
                    currentFOV -= mouseInputY * zoomSpeed * -1;
                    currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
                    ApplyFOV();
                }
            }
        }

        private void ApplyFOV()
        {
            // "Pour la caméra manuelle, je reste sur le FOV pur de l'utilisateur."
            if (_manualCamComponent != null) _manualCamComponent.fieldOfView = currentFOV;

            // "Pour l'auto, je combine le réglage de base et ma micro-variation organique."
            if (_autoCamComponent != null) 
            {
                _autoCamComponent.fieldOfView = currentFOV + _dynamicFOVOffset;
            }
        }
        #endregion

        #region Logique des Caméras

        private void RunManualLogic()
        {
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
            Vector3 localPoint = new Vector3(x, autoHeight, z);

            Quaternion tilt = Quaternion.Euler(autoOrbitTiltX, 0, 0);
            Vector3 tiltedPoint = tilt * localPoint;

            _autoCamera.transform.position = _autoOrbitCenter + tiltedPoint;

            if (_lookAtTarget != null)
            {
                _autoCamera.transform.LookAt(_currentLookAtPos);
            }
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
            if (_lookAtTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_lookAtTarget.position, 0.2f);
                
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(_currentLookAtPos, 0.1f);

                if (_manualCamComponent != null) Gizmos.DrawLine(_manualCamComponent.transform.position, _currentLookAtPos);
                if (_autoCamera != null) Gizmos.DrawLine(_autoCamera.transform.position, _currentLookAtPos);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_manualOrbitCenter, Vector3.one * 0.5f);
            Gizmos.DrawWireSphere(_autoOrbitCenter, 0.5f);

            Gizmos.color = Color.cyan;
            Vector3 lastPoint = Vector3.zero;
            Quaternion tilt = Quaternion.Euler(autoOrbitTiltX, 0, 0);

            for (int i = 0; i <= 64; i++)
            {
                float stepAngle = i * Mathf.PI * 2 / 64;
                Vector3 localStep = new Vector3(Mathf.Cos(stepAngle) * autoRadius, autoHeight, Mathf.Sin(stepAngle) * autoRadius);
                Vector3 tiltedStep = tilt * localStep;
                Vector3 pos = _autoOrbitCenter + tiltedStep;

                if (i > 0) Gizmos.DrawLine(lastPoint, pos);
                lastPoint = pos;
            }
        }

        #endregion
    }
}