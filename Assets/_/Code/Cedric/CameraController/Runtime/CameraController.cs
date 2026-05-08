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

        // --- NOUVEAU : RÉGLAGES D'INACTIVITÉ ---
        [Header("Réglages Inactivité (Auto-Idle)")]
        [Tooltip("Temps sans input (souris/clavier) avant le passage en caméra auto.")]
        public float idleThreshold = 10f; 
        [Tooltip("Si activé, le moindre mouvement de souris repasse en mode manuel.")]
        public bool breakAutoOnActivity = true;

        [Header("Réglages Dynamic FOV (Auto uniquement)")]
        public float dynamicFOVAmount = 1.5f;
        public float dynamicFOVAmountRandomness = 1.0f;
        public float dynamicFOVInterval = 3f;
        public float dynamicFOVRandomness = 2f;
        public float dynamicFOVSmoothness = 2f;

        [Header("Réglages Zoom (Molette / Bouton Milieu)")]
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
        private float _idleTimer = 0f; 
        private CameraTransition _transitionEffect; 
        #endregion

        #region Unity API
        private void Start()
        {
            // "Je cherche si un script de transition est présent sur le même objet."
            _transitionEffect = GetComponent<CameraTransition>();
            // "Je récupère mes composants caméras pour agir sur leur FOV plus tard."
            if (_manualPivot != null) _manualCamComponent = _manualPivot.GetComponentInChildren<Camera>();
            if (_autoCamera != null) _autoCamComponent = _autoCamera.GetComponent<Camera>();
            
            // "J'initialise les états de visibilité des caméras (Auto vs Manuel)."
            ApplyCameraSwitch();
            ApplyFOV();
            
            // "Je définis les positions de départ pour que le premier Lerp ne soit pas brutal."
            _targetOrbitPosition = _manualOrbitCenter;
            if (_lookAtTarget != null) _currentLookAtPos = _lookAtTarget.position;
            if (_manualPivot != null) _manualPivot.transform.position = _manualOrbitCenter;
            
            // "Je prépare déjà le premier 'pulse' du FOV dynamique."
            ResetPulseTimer();
        }

        private void Update()
        {
            // --- GESTION DU TEMPS ET DES INPUTS ---
            HandleInactivityLogic();

            // --- GESTION DES LERPS DE POSITION ---
            // "Je fais glisser les centres d'orbite vers leur cible actuelle (souvent une roue sélectionnée)."
            _manualOrbitCenter = Vector3.Lerp(_manualOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);
            _autoOrbitCenter = Vector3.Lerp(_autoOrbitCenter, _targetOrbitPosition, Time.deltaTime * centerLerpSpeed);

            // "Je fais glisser le point de regard vers la cible pour un mouvement fluide de la tête de caméra."
            if (_lookAtTarget != null)
            {
                _currentLookAtPos = Vector3.Lerp(_currentLookAtPos, _lookAtTarget.position, Time.deltaTime * lookAtLerpSpeed);
            }

            // --- GESTION DU FOV ---
            if (_isAutoActive) 
            {
                // "Si on est en auto, je gère la respiration organique du FOV."
                HandleDynamicFOV();
            }
            else 
            {
                // "Si on est en manuel, je m'assure que tout décalage du FOV dynamique revient à zéro."
                _targetDynamicOffset = 0f;
                _dynamicFOVOffset = Mathf.Lerp(_dynamicFOVOffset, 0f, Time.deltaTime * dynamicFOVSmoothness);
                ApplyFOV();
            }

            // "Je vérifie si le joueur utilise la molette ou le clic milieu pour zoomer."
            HandleZoom();

            // "La barre espace reste le bouton d'urgence pour forcer le passage d'un mode à l'autre."
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ToggleAutoCamera();
            }

            // --- EXÉCUTION DE LA LOGIQUE DE CAMÉRA ---
            if (_isAutoActive) RunAutoLogic();
            else RunManualLogic();
        }
        #endregion

        #region Logique d'Inactivité (IDLE)

        private void HandleInactivityLogic()
        {
            // "Je vais scruter les moindres faits et gestes du joueur."
            
            // "Check 1 : Est-ce qu'une touche du clavier est enfoncée ?"
            bool isKeyboardPressed = Keyboard.current.anyKey.isPressed;
            
            // "Check 2 : Est-ce que la souris bouge ?" 
            // "J'utilise sqrMagnitude car c'est plus performant pour comparer une longueur de vecteur."
            bool isMouseMoving = Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f;

            // "Check 3 : Est-ce que l'un des boutons de la souris est enfoncé ?"
            bool isMousePressed = Mouse.current.leftButton.isPressed || 
                                 Mouse.current.rightButton.isPressed || 
                                 Mouse.current.middleButton.isPressed;

            if (isKeyboardPressed || isMouseMoving || isMousePressed)
            {
                // "DÈS QU'IL SE PASSE QUELQUE CHOSE :"
                
                // "1. Je remets mon compteur d'inactivité à zéro immédiatement."
                _idleTimer = 0f;

                // "2. Si j'étais en mode auto et que 'breakAutoOnActivity' est vrai,"
                // "alors je repousse la caméra auto pour redonner la main au joueur."
                if (_isAutoActive && breakAutoOnActivity)
                {
                    ToggleAutoCamera();
                }
            }
            else
            {
                // "SI LE JOUEUR NE TOUCHE À RIEN :"
                
                // "J'incrémente mon chronomètre interne."
                _idleTimer += Time.deltaTime;

                // "Si on a dépassé le seuil (ex: 10 sec) et qu'on n'est pas encore en auto,"
                // "alors j'active le mode automatique tout seul."
                if (_idleTimer >= idleThreshold && !_isAutoActive)
                {
                    ToggleAutoCamera();
                }
            }
        }

        private void ToggleAutoCamera()
        {
            // "MODIFICATION : Au lieu de switcher direct, je demande au script de transition d'agir."
            // "Si j'ai un script de transition, je lance l'effet. Sinon, je switch instantanément comme avant."
            if (_transitionEffect != null)
            {
                // "Je passe l'action 'ExecuteInternalSwitch' en paramètre pour qu'elle soit exécutée au milieu du fade."
                _transitionEffect.StartTransition(ExecuteInternalSwitch);
            }
            else
            {
                ExecuteInternalSwitch();
            }
        }
        // "J'isole le vrai switch dans sa propre fonction pour qu'il puisse être appelé par le script de transition."
        public void ExecuteInternalSwitch()
        {
            _isAutoActive = !_isAutoActive;
            _idleTimer = 0f;
            ApplyCameraSwitch();
        }

        #endregion

        #region Logique Dynamic FOV (Respiration Organique)

        private void HandleDynamicFOV()
        {
            _nextPulseTimer -= Time.deltaTime;

            if (_nextPulseTimer <= 0f)
            {
                // "Je lance un dé pour savoir si on zoome ou dézoome."
                float randomSign = (Random.value > 0.5f) ? 1f : -1f;
                // "Je calcule une force aléatoire."
                float randomForce = dynamicFOVAmount + Random.Range(0f, dynamicFOVAmountRandomness);
                _targetDynamicOffset = randomSign * randomForce;
                
                ResetPulseTimer();
            }

            // "Si je suis proche du pic du mouvement, je demande un retour vers le zéro."
            if (Mathf.Abs(_dynamicFOVOffset - _targetDynamicOffset) < 0.05f)
            {
                _targetDynamicOffset = 0f;
            }

            // "Lerp constant pour que le changement de FOV ne soit jamais brusque."
            _dynamicFOVOffset = Mathf.Lerp(_dynamicFOVOffset, _targetDynamicOffset, Time.deltaTime * dynamicFOVSmoothness);
            ApplyFOV();
        }

        private void ResetPulseTimer()
        {
            _nextPulseTimer = dynamicFOVInterval + Random.Range(0f, dynamicFOVRandomness);
        }

        #endregion

        #region Focus & Zoom

        public void UpdateCameraFocus(Vector3 newPoint, Transform newTarget)
        {
            // "Cette fonction me permet de dire à la caméra : 'Regarde cet objet et orbite autour de ce point'."
            _lookAtTarget = newTarget;
            _targetOrbitPosition = newPoint;
        }

        private void HandleZoom()
        {
            // "Gestion du zoom par Drag (Bouton milieu)."
            if (Mouse.current.middleButton.isPressed)
            {
                float mouseInputY = Mouse.current.delta.ReadValue().y;
                if (mouseInputY != 0)
                {
                    currentFOV -= mouseInputY * zoomSpeed * -1;
                }
            }

            // "Gestion du zoom par Molette."
            float scrollInput = Mouse.current.scroll.ReadValue().y;
            if (scrollInput != 0)
            {
                // "Le 0.05f sert à calmer la puissance de la molette qui renvoie souvent des valeurs énormes."
                currentFOV -= scrollInput * (zoomSpeed * 4.0f);
            }

            currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
            ApplyFOV();
        }

        private void ApplyFOV()
        {
            if (_manualCamComponent != null) _manualCamComponent.fieldOfView = currentFOV;

            if (_autoCamComponent != null) 
            {
                // "C'est ici que j'additionne le FOV choisi par le joueur et ma petite variation automatique."
                _autoCamComponent.fieldOfView = currentFOV + _dynamicFOVOffset;
            }
        }
        #endregion

        #region Logique des Caméras (Run)

        private void RunManualLogic()
        {
            // "Je place le pivot manuel sur le centre d'orbite fluide."
            _manualPivot.transform.position = _manualOrbitCenter;

            // "Si le bouton droit est maintenu, je tourne autour de l'objet."
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                
                // "Rotation Horizontale (Axe Y)."
                _manualPivot.transform.Rotate(Vector3.up, delta.x * manualRotationSpeed, Space.World);

                // "Rotation Verticale (Axe X) avec Clamp pour ne pas finir la tête à l'envers."
                _verticalRotation -= delta.y * manualRotationSpeed;
                _verticalRotation = Mathf.Clamp(_verticalRotation, minVerticalAngle, maxVerticalAngle);

                float currentY = _manualPivot.transform.localEulerAngles.y;
                _manualPivot.transform.localRotation = Quaternion.Euler(_verticalRotation, currentY, 0);
            }

            // "Je force la caméra à toujours regarder le point cible (LookAt)."
            if (_manualCamComponent != null)
            {
                _manualCamComponent.transform.LookAt(_currentLookAtPos);
            }
        }

        private void RunAutoLogic()
        {
            // "Je fais tourner l'angle mathématique du cercle."
            _autoAngle += autoSpeed * Time.deltaTime;

            // "Calcul Trigonométrique pour placer la caméra sur un cercle parfait."
            float x = Mathf.Cos(_autoAngle) * autoRadius;
            float z = Mathf.Sin(_autoAngle) * autoRadius;
            Vector3 localPoint = new Vector3(x, autoHeight, z);

            // "J'applique l'inclinaison X (Tilt) au cercle pour un effet plus stylé."
            Quaternion tilt = Quaternion.Euler(autoOrbitTiltX, 0, 0);
            Vector3 tiltedPoint = tilt * localPoint;

            // "Je positionne la caméra auto par rapport au centre fluide."
            _autoCamera.transform.position = _autoOrbitCenter + tiltedPoint;

            // "Regard fluide vers la cible."
            if (_lookAtTarget != null)
            {
                _autoCamera.transform.LookAt(_currentLookAtPos);
            }
        }

        #endregion

        #region Helpers & Gizmos

        private void ApplyCameraSwitch()
        {
            // "J'allume la caméra auto et j'éteins la manuelle (ou inversement)."
            if (_autoCamera != null) _autoCamera.SetActive(_isAutoActive);
            if (_manualPivot != null) _manualPivot.SetActive(!_isAutoActive);
        }

        private void OnDrawGizmos()
        {
            // "C'est ici que je dessine les aides visuelles dans l'éditeur Unity pour m'y retrouver."
            if (_lookAtTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_lookAtTarget.position, 0.2f);
                
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(_currentLookAtPos, 0.1f);

                if (_manualCamComponent != null) Gizmos.DrawLine(_manualCamComponent.transform.position, _currentLookAtPos);
                if (_autoCamera != null) Gizmos.DrawLine(_autoCamera.transform.position, _currentLookAtPos);
            }

            // "Je dessine les centres d'orbite (Jaune)."
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_manualOrbitCenter, Vector3.one * 0.5f);
            Gizmos.DrawWireSphere(_autoOrbitCenter, 0.5f);

            // "Je dessine le rail circulaire de la caméra auto (Cyan)."
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