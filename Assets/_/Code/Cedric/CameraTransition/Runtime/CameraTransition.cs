using UnityEngine;
using UnityEngine.UI;

namespace CameraController.Runtime
{
    public class CameraTransition : MonoBehaviour
    {
        // "Je garde mon menu déroulant pour choisir le style dans l'inspecteur."
        public enum TransitionType { FadeAlpha, SlideY }

        // "Je crée les étapes de ma machine à états pour savoir où j'en suis dans le temps."
        private enum TransitionState { Idle, MovingOut, MovingIn }

        [Header("Réglages de l'Effet")]
        [SerializeField] private TransitionType _type = TransitionType.FadeAlpha;
        
        [Tooltip("Durée totale de la transition (FadeOut + FadeIn).")]
        public float totalDuration = 1.0f;
        
        [Tooltip("Couleur du panneau de transition.")]
        public Color transitionColor = Color.black;

        [Header("Références UI")]
        public Image transitionImage;

        // --- VARIABLES DE CONTRÔLE INTERNE ---
        private TransitionState _currentState = TransitionState.Idle; // "Mon état actuel."
        private float _timer = 0f;                                    // "Mon chronomètre manuel."
        private RectTransform _imageRect;                             // "Pour le mouvement Y."
        private System.Action _onMidPointAction;                      // "L'action (le switch) à faire au milieu."

        private void Awake()
        {
            // "J'initialise mes composants comme avant."
            if (transitionImage != null)
            {
                _imageRect = transitionImage.GetComponent<RectTransform>();
                transitionImage.color = transitionColor;
                ResetUI();
            }
        }

        private void Update()
        {
            // "Si je suis au repos (Idle), je ne fais rien du tout pour économiser de la ressource."
            if (_currentState == TransitionState.Idle) return;

            // "Je fais avancer mon chronomètre en ajoutant le temps écoulé depuis la dernière image."
            _timer += Time.deltaTime;

            // "Je calcule la durée d'une demi-transition."
            float halfDuration = totalDuration / 2f;

            if (_currentState == TransitionState.MovingOut)
            {
                // "--- PHASE 1 : ON CACHE L'ÉCRAN ---"
                // "Je calcule un ratio de progression entre 0 et 1 pour la première moitié du temps."
                float progress = Mathf.Clamp01(_timer / halfDuration);
                
                ApplyEffectLogic(progress, true);

                // "Si mon chrono dépasse la moitié de la durée totale..."
                if (_timer >= halfDuration)
                {
                    // "1. Je m'assure d'être parfaitement à l'état 'caché' (noir total ou centré)."
                    ApplyEffectLogic(1f, true);

                    // "2. J'EXÉCUTE LE SWITCH DE CAMÉRA (Le moment critique !)."
                    _onMidPointAction?.Invoke();

                    // "3. Je passe à la phase suivante : On révèle l'écran."
                    _currentState = TransitionState.MovingIn;
                    
                    // "4. Je remets mon chrono à zéro pour la deuxième phase."
                    _timer = 0f;
                }
            }
            else if (_currentState == TransitionState.MovingIn)
            {
                // "--- PHASE 2 : ON RÉVÈLE L'ÉCRAN ---"
                // "Je calcule le ratio de progression pour la seconde moitié."
                float progress = Mathf.Clamp01(_timer / halfDuration);

                ApplyEffectLogic(progress, false);

                // "Si mon chrono dépasse la deuxième moitié, c'est fini."
                if (_timer >= halfDuration)
                {
                    _currentState = TransitionState.Idle;
                    ResetUI();
                }
            }
        }

        // "Cette fonction centralise la transformation visuelle pour éviter de répéter du code."
        private void ApplyEffectLogic(float progress, bool isMovingOut)
        {
            if (_type == TransitionType.FadeAlpha)
            {
                // "Si je sors (Out), je vais de 0 vers 1. Si je rentre (In), je vais de 1 vers 0."
                float startAlpha = isMovingOut ? 0f : 1f;
                float endAlpha = isMovingOut ? 1f : 0f;
                SetAlpha(Mathf.Lerp(startAlpha, endAlpha, progress));
            }
            else
            {
                // "Si je sors (Out), je monte du bas vers le centre (-540 vers 0)."
                // "Si je rentre (In), je pars du centre vers le haut (0 vers 540)."
                float startY = isMovingOut ? -540f : 0f;
                float endY = isMovingOut ? 0f : 540f;
                float currentY = Mathf.Lerp(startY, endY, progress);
                _imageRect.anchoredPosition = new Vector2(0, currentY);
            }
        }

        // "La porte d'entrée appelée par le CameraController."
        public void StartTransition(System.Action onMidPointReached)
        {
            // "Si une transition tourne déjà, j'ignore la demande."
            if (_currentState != TransitionState.Idle) return;

            // "Je stocke l'action de switch pour plus tard."
            _onMidPointAction = onMidPointReached;

            // "Je lance la machine à états et je remets le chrono à zéro."
            _currentState = TransitionState.MovingOut;
            _timer = 0f;
        }

        private void ResetUI()
        {
            if (_type == TransitionType.FadeAlpha)
                SetAlpha(0f);
            else
                _imageRect.anchoredPosition = new Vector2(0, -540);
        }

        private void SetAlpha(float alpha)
        {
            if (transitionImage == null) return;
            Color c = transitionImage.color;
            c.a = alpha;
            transitionImage.color = c;
        }
    }
}