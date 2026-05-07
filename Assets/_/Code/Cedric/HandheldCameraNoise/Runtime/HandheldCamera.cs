using UnityEngine;

namespace CameraController.Runtime
{
    public class HandheldCameraFBm : MonoBehaviour
    {
        #region Paramètres fBm (La "Texture" du mouvement)

        [Header("Réglages du Bruit Fractal (fBm)")]
        [Tooltip("Nombre de couches de bruit. Plus c'est élevé, plus c'est 'nerveux' et détaillé.")]
        [Range(1, 8)] public int octaves = 3;
        
        [Tooltip("Vitesse globale de l'effet.")]
        public float noiseSpeed = 0.5f;

        [Tooltip("Multiplicateur de fréquence entre chaque octave. (Sensation de détails).")]
        public float lacunarity = 2.0f;

        [Tooltip("Multiplicateur d'amplitude entre chaque octave. (Poids des détails).")]
        [Range(0, 1)] public float persistence = 0.5f;

        #endregion

        #region Intensités par Axe

        [Header("Intensité Position (Drift)")]
        [Tooltip("Force du balancement sur les axes X, Y et Z.")]
        public Vector3 positionAmount = new Vector3(0.05f, 0.05f, 0.05f);

        [Header("Intensité Rotation (Épaule)")]
        [Tooltip("Force de l'oscillation sur les axes X (Pitch), Y (Yaw) et Z (Roll).")]
        public Vector3 rotationAmount = new Vector3(0.5f, 0.5f, 0.5f);

        #endregion

        #region Variables de Calcul Internes

        // Je génère des seeds uniques pour que chaque axe ait son propre mouvement indépendant
        private float[] _seeds = new float[6];

        #endregion

        private void Start()
        {
            // "Je prépare 6 points de départ différents dans la carte du bruit de Perlin."
            // "3 pour la position, 3 pour la rotation. Comme ça, le X ne bouge pas comme le Y."
            for (int i = 0; i < 6; i++)
            {
                _seeds[i] = Random.value * 1000f;
            }
        }

        private void LateUpdate()
        {
            float time = Time.time * noiseSpeed;

            // --- 1. CALCUL DES DÉPLACEMENTS POSITIONNELS ---
            // "J'appelle ma fonction fBm pour chaque axe en utilisant leurs seeds respectives."
            float noisePosX = CalculateFBm(_seeds[0], time) * positionAmount.x;
            float noisePosY = CalculateFBm(_seeds[1], time) * positionAmount.y;
            float noisePosZ = CalculateFBm(_seeds[2], time) * positionAmount.z;

            transform.position += new Vector3(noisePosX, noisePosY, noisePosZ);

            // --- 2. CALCUL DES MICRO-ROTATIONS ---
            float noiseRotX = CalculateFBm(_seeds[3], time) * rotationAmount.x;
            float noiseRotY = CalculateFBm(_seeds[4], time) * rotationAmount.y;
            float noiseRotZ = CalculateFBm(_seeds[5], time) * rotationAmount.z;

            // "J'applique cette rotation en local par-dessus ce que le CameraController a décidé."
            transform.localRotation *= Quaternion.Euler(noiseRotX, noiseRotY, noiseRotZ);
        }

        /// <summary>
        /// Fonction magique du Fractal Brownian Motion.
        /// Elle cumule plusieurs octaves de Perlin Noise.
        /// </summary>
        private float CalculateFBm(float seed, float time)
        {
            float total = 0f;
            float currentFrequency = 1f;
            float currentAmplitude = 1f;
            float maxValue = 0f; // Pour normaliser le résultat à la fin

            for (int i = 0; i < octaves; i++)
            {
                // "Je lis la valeur de Perlin, je la remets entre -1 et 1, et j'applique l'amplitude."
                total += (Mathf.PerlinNoise(seed, time * currentFrequency) * 2f - 1f) * currentAmplitude;
                
                maxValue += currentAmplitude;
                
                // "Pour la prochaine couche, j'augmente la fréquence (lacunarity) et je baisse la force (persistence)."
                currentFrequency *= lacunarity;
                currentAmplitude *= persistence;
            }

            // "Je divise par le maximum possible pour garder un contrôle précis sur mes 'Amount' dans l'inspecteur."
            return total / maxValue;
        }
    }
}