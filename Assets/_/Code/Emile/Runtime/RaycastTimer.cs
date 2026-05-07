using UnityEngine;
using UnityEngine.UI;

namespace FixPoint.Runtime
{
    public class RaycastTimer : MonoBehaviour
    {
        [SerializeField] private LayerMask obstaclemask;
        [SerializeField] private LayerMask layermask;
        [SerializeField] private Slider scanSlider;

        private RaycastHit hitinfo;

        private float timer;
        private float maxTime = 3f;
        
        
        void Start()
        {
            scanSlider.gameObject.SetActive(false);
        }

        void Update()
        {
            Vector3 direction = transform.forward;
            if (Physics.Raycast(transform.position, direction, out hitinfo, 10f, obstaclemask)) return;

            if (Physics.Raycast(transform.position, direction, out hitinfo, 10f, layermask))
            {
                scanSlider.gameObject.SetActive(true);

                timer += Time.deltaTime;

                scanSlider.value = timer;

                Debug.Log("Scan in progress");

                if (timer >= maxTime)
                {
                    Debug.Log("Object scanned !");
                }

                Debug.DrawRay(
                    transform.position,
                    direction * hitinfo.distance,
                    Color.red
                );
            }
            else
            {
                timer = 0;

                scanSlider.value = 0;

                scanSlider.gameObject.SetActive(false);

                Debug.Log("No objects scanned");

                Debug.DrawRay(
                    transform.position,
                    direction * 10f,
                    Color.green
                );
            }
        }
    }
}