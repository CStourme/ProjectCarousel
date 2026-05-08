using UnityEngine;
using UnityEngine.UI;

namespace CameraController.Runtime
{
    public class CameraTransition : MonoBehaviour
    {
        public enum TransitionType { FadeAlpha, Slide }
        private enum TransitionState { Idle, Running }

        [Header("Réglages de l'Effet")]
        [SerializeField] private TransitionType _type = TransitionType.FadeAlpha;
        public float totalDuration = 1.0f;
        public Color transitionColor = Color.black;

        [Header("Références UI")]
        public Image imageBottom;
        public Image imageTop;

        [Header("Override de Couleur (Mode Slide)")]
        public Color colorBottomOverride = Color.white;
        public Color colorTopOverride = Color.white;

        [Header("Positions (Mode Slide)")]
        [Tooltip("Position de départ (Hors-écran).")]
        public Vector2 bottomStartPos = new Vector2(0, -540);
        [Tooltip("Position après le switch (Retour hors-écran). Si tu mets (0,0) l'image restera au centre !")]
        public Vector2 bottomEndPos = new Vector2(0, 540);
        [Space]
        public Vector2 topStartPos = new Vector2(0, 540);
        public Vector2 topEndPos = new Vector2(0, -540);

        // --- VARIABLES DE CONTRÔLE ---
        private TransitionState _currentState = TransitionState.Idle;
        private float _timer = 0f;
        private bool _hasSwitchedCamera = false; // "Petit verrou pour ne pas switcher 50 fois par frame."
        private RectTransform _rectBottom;
        private RectTransform _rectTop;
        private System.Action _onMidPointAction;

        private void Awake()
        {
            if (imageBottom != null) _rectBottom = imageBottom.GetComponent<RectTransform>();
            if (imageTop != null) _rectTop = imageTop.GetComponent<RectTransform>();
            ResetUI();
        }

        private void Update()
        {
            if (_currentState == TransitionState.Idle) return;

            // "1. J'avance mon chrono unique de 0 à totalDuration."
            _timer += Time.deltaTime;

            // "2. Je calcule où j'en suis globalement (de 0.0 à 1.0)."
            float globalProgress = Mathf.Clamp01(_timer / totalDuration);

            // "3. GESTION DU SWITCH (Le point critique à 50% / 0.5)."
            if (globalProgress >= 0.5f && !_hasSwitchedCamera)
            {
                _onMidPointAction?.Invoke();
                _hasSwitchedCamera = true; // "C'est bon, le switch est fait, je ferme le verrou."
            }

            // "4. CALCUL DE L'ANIMATION."
            // "Au lieu de deux phases, je décide de ce que je fais selon si je suis avant ou après 0.5."
            if (globalProgress < 0.5f)
            {
                // "PHASE ALLER (0.0 à 0.5) -> Je re-mappe cette valeur entre 0 et 1."
                float phaseProgress = globalProgress / 0.5f;
                ApplyAnimation(phaseProgress, true);
            }
            else
            {
                // "PHASE RETOUR (0.5 à 1.0) -> Je re-mappe cette valeur entre 0 et 1."
                float phaseProgress = (globalProgress - 0.5f) / 0.5f;
                ApplyAnimation(phaseProgress, false);
            }

            // "5. FIN DE LA TRANSITION."
            if (globalProgress >= 1f)
            {
                _currentState = TransitionState.Idle;
                ResetUI();
            }
        }

        private void ApplyAnimation(float progress, bool isMovingIn)
        {
            if (_type == TransitionType.FadeAlpha)
            {
                // "FADE : Aller (0->1), Retour (1->0)."
                float alpha = isMovingIn ? Mathf.Lerp(0f, 1f, progress) : Mathf.Lerp(1f, 0f, progress);
                SetColorAndAlpha(imageBottom, transitionColor, alpha);
                SetColorAndAlpha(imageTop, transitionColor, alpha);
            }
            else
            {
                // "SLIDE : Je force l'opacité pour mes bitmaps."
                SetColorAndAlpha(imageBottom, colorBottomOverride, 1f);
                SetColorAndAlpha(imageTop, colorTopOverride, 1f);

                // "IMAGE BAS : Aller (Start -> Centre), Retour (Centre -> End)."
                Vector2 posB = isMovingIn ? Vector2.Lerp(bottomStartPos, Vector2.zero, progress) 
                                          : Vector2.Lerp(Vector2.zero, bottomEndPos, progress);
                if (_rectBottom != null) _rectBottom.anchoredPosition = posB;

                // "IMAGE HAUT : Aller (Start -> Centre), Retour (Centre -> End)."
                Vector2 posT = isMovingIn ? Vector2.Lerp(topStartPos, Vector2.zero, progress) 
                                          : Vector2.Lerp(Vector2.zero, topEndPos, progress);
                if (_rectTop != null) _rectTop.anchoredPosition = posT;
            }
        }

        public void StartTransition(System.Action onMidPointReached)
        {
            if (_currentState != TransitionState.Idle) return;
            _onMidPointAction = onMidPointReached;
            _hasSwitchedCamera = false; // "Je réinitialise le verrou."
            _timer = 0f;
            _currentState = TransitionState.Running;
        }

        private void ResetUI()
        {
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

        private void SetColorAndAlpha(Image img, Color targetColor, float alpha)
        {
            if (img == null) return;
            Color finalColor = targetColor;
            finalColor.a = alpha;
            img.color = finalColor;
        }
    }
}