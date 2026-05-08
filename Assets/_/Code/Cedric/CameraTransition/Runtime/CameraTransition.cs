using UnityEngine;
using UnityEngine.UI;

namespace CameraController.Runtime
{
    public class CameraTransition : MonoBehaviour
    {
        public enum TransitionType { FadeAlpha, Slide }
        private enum TransitionState { Idle, MovingOut, MovingIn }

        [Header("Réglages de l'Effet")]
        [SerializeField] private TransitionType _type = TransitionType.FadeAlpha;
        
        [Tooltip("Durée totale (Fermeture + Ouverture).")]
        public float totalDuration = 1.0f;
        
        [Tooltip("Utilisé UNIQUEMENT en mode FadeAlpha.")]
        public Color transitionColor = Color.black;

        [Header("Références UI")]
        public Image imageBottom;
        public Image imageTop;

        // --- NOUVEAU : OVERRIDE DE COULEUR POUR LES BITMAPS ---
        [Header("Override de Couleur (Mode Slide)")]
        [Tooltip("Couleur appliquée aux pixels de la texture du bas/gauche.")]
        public Color colorBottomOverride = Color.white;
        [Tooltip("Couleur appliquée aux pixels de la texture du haut/droite.")]
        public Color colorTopOverride = Color.white;

        [Header("Positions (Mode Slide)")]
        public Vector2 bottomStartPos = new Vector2(0, -540);
        public Vector2 bottomEndPos = new Vector2(0, 540);
        [Space]
        public Vector2 topStartPos = new Vector2(0, 540);
        public Vector2 topEndPos = new Vector2(0, -540);

        private TransitionState _currentState = TransitionState.Idle;
        private float _timer = 0f;
        private RectTransform _rectBottom;
        private RectTransform _rectTop;
        private System.Action _onMidPointAction;

        private void Awake()
        {
            // "Je récupère les composants de mes images."
            if (imageBottom != null) _rectBottom = imageBottom.GetComponent<RectTransform>();
            if (imageTop != null) _rectTop = imageTop.GetComponent<RectTransform>();

            // "Initialisation : je prépare les couleurs et je masque tout."
            ResetUI();
        }

        private void Update()
        {
            // "Si je ne fais rien, je sors."
            if (_currentState == TransitionState.Idle) return;

            // "Progression du chrono."
            _timer += Time.deltaTime;
            float halfDuration = totalDuration / 2f;

            if (_currentState == TransitionState.MovingOut)
            {
                // "PHASE 1 : FERMETURE."
                float progress = Mathf.Clamp01(_timer / halfDuration);
                ApplyEffectLogic(progress, true);

                if (_timer >= halfDuration)
                {
                    ApplyEffectLogic(1f, true); 
                    _onMidPointAction?.Invoke(); // "Switch caméra ici."
                    _currentState = TransitionState.MovingIn;
                    _timer = 0f;
                }
            }
            else if (_currentState == TransitionState.MovingIn)
            {
                // "PHASE 2 : OUVERTURE."
                float progress = Mathf.Clamp01(_timer / halfDuration);
                ApplyEffectLogic(progress, false);

                if (_timer >= halfDuration)
                {
                    _currentState = TransitionState.Idle;
                    ResetUI();
                }
            }
        }

        private void ApplyEffectLogic(float progress, bool isMovingOut)
        {
            if (_type == TransitionType.FadeAlpha)
            {
                // "MODE FADE : J'utilise la couleur 'transitionColor' pour les deux."
                float startAlpha = isMovingOut ? 0f : 1f;
                float endAlpha = isMovingOut ? 1f : 0f;
                float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                
                SetColorAndAlpha(imageBottom, transitionColor, currentAlpha);
                SetColorAndAlpha(imageTop, transitionColor, currentAlpha);
            }
            else
            {
                // "MODE SLIDE : J'applique les couleurs d'override spécifiques à chaque image."
                // "Je force l'alpha à 1 car l'opacité doit être totale pour masquer le switch."
                SetColorAndAlpha(imageBottom, colorBottomOverride, 1f);
                SetColorAndAlpha(imageTop, colorTopOverride, 1f);

                // "Calcul des trajectoires Vector2."
                Vector2 startB = isMovingOut ? bottomStartPos : Vector2.zero;
                Vector2 endB = isMovingOut ? Vector2.zero : bottomEndPos;
                if (_rectBottom != null) _rectBottom.anchoredPosition = Vector2.Lerp(startB, endB, progress);

                Vector2 startT = isMovingOut ? topStartPos : Vector2.zero;
                Vector2 endT = isMovingOut ? Vector2.zero : topEndPos;
                if (_rectTop != null) _rectTop.anchoredPosition = Vector2.Lerp(startT, endT, progress);
            }
        }

        public void StartTransition(System.Action onMidPointReached)
        {
            if (_currentState != TransitionState.Idle) return;
            _onMidPointAction = onMidPointReached;
            _currentState = TransitionState.MovingOut;
            _timer = 0f;
        }

        private void ResetUI()
        {
            // "Au repos, j'applique les couleurs de base pour que l'initialisation soit propre."
            if (_type == TransitionType.Slide)
            {
                SetColorAndAlpha(imageBottom, colorBottomOverride, 1f);
                SetColorAndAlpha(imageTop, colorTopOverride, 1f);
            }
            else
            {
                SetColorAndAlpha(imageBottom, transitionColor, 0f);
                SetColorAndAlpha(imageTop, transitionColor, 0f);
            }

            if (_rectBottom != null) _rectBottom.anchoredPosition = bottomStartPos;
            if (_rectTop != null) _rectTop.anchoredPosition = topStartPos;
        }

        // "--- NOUVELLE MÉTHODE PÉDAGOGIQUE ---"
        // "Cette fonction permet de changer à la fois la teinte ET la transparence d'un coup."
        private void SetColorAndAlpha(Image img, Color targetColor, float alpha)
        {
            if (img == null) return;
            
            // "Je crée une copie de la couleur cible."
            Color finalColor = targetColor;
            
            // "J'injecte la valeur d'alpha demandée par la machine à états."
            finalColor.a = alpha;
            
            // "J'applique cette couleur finale à l'image."
            // "Si l'image a une texture bitmap blanche, elle prendra exactement cette couleur."
            // "Si l'image a déjà des couleurs, elle sera teintée (multipliée) par celle-ci."
            img.color = finalColor;
        }
    }
}