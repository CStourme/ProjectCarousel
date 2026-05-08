using System;
using UnityEngine;
using UnityEngine.UI;

namespace FixPoint.Runtime
{
    public class RaycastTimer : MonoBehaviour
    {
        #region Publics

        public Action<GameObject> OnScanComplete;

        #endregion


        #region Inspector

        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Slider scanSlider;

        #endregion


        #region Privates

        private RaycastHit _hitInfo;

        private float _timer;
        private float _maxTime = 1f;

        private bool _scanCompleted;

        #endregion


        #region Unity API

        private void Start()
        {
            scanSlider.gameObject.SetActive(false);
            scanSlider.maxValue = _maxTime;
        }

        private void Update()
        {
            Vector3 direction = transform.forward;

            // Bloque le scan si obstacle
            if (Physics.Raycast(transform.position, direction, out _hitInfo, 10f, obstacleMask))
            {
                ResetScan();
                return;
            }

            // Vérifie si on touche un objet du layer
            if (Physics.Raycast(transform.position, direction, out _hitInfo, 10f, targetMask))
            {
                scanSlider.gameObject.SetActive(true);

                _timer += Time.deltaTime;

                scanSlider.value = _timer;

                Debug.Log("Scan in progress");

                // Déclenche UNE seule fois
                if (_timer >= _maxTime && !_scanCompleted)
                {
                    _scanCompleted = true;

                    Debug.Log("Object scanned !");

                    OnScanComplete?.Invoke(_hitInfo.collider.gameObject);
                }

                Debug.DrawRay(
                    transform.position,
                    direction * _hitInfo.distance,
                    Color.red
                );
            }
            else
            {
                ResetScan();

                Debug.DrawRay(
                    transform.position,
                    direction * 10f,
                    Color.green
                );
            }
        }

        #endregion


        #region Main Methods

        private void ResetScan()
        {
            _timer = 0;

            _scanCompleted = false;

            scanSlider.value = 0;

            scanSlider.gameObject.SetActive(false);

            Debug.Log("No objects scanned");
        }

        #endregion
    }
}