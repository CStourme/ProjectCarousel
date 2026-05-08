using UnityEngine;
using TMPro;

namespace FixPoint.Runtime
{
    public class FixPoint : MonoBehaviour
    {
        [SerializeField] private LayerMask obstaclemask;
        [SerializeField] private LayerMask layermask;
        // [SerializeField] private TMP_Text detectionText;

        private RaycastHit hitinfo;

        void Update()
        {
            Vector3 direction = transform.forward;
            
            if (Physics.Raycast(transform.position, direction, out hitinfo, 10f, obstaclemask)) return;

            if (Physics.Raycast(transform.position, direction, out hitinfo, 10f, layermask))
            {
                // detectionText.text = "Object detected : " + hitinfo.collider.name;
                // detectionText.color = Color.green;

                Debug.Log("Object detected : " + hitinfo.collider.name);

                Debug.DrawRay(
                    transform.position,
                    direction * hitinfo.distance,
                    Color.red
                );
            }
            else
            {
                // detectionText.text = "No objects detected";
                // detectionText.color = Color.red;

                Debug.Log("No objects detected");

                Debug.DrawRay(
                    transform.position,
                    direction * 10f,
                    Color.green
                );
            }
            
        }
    }
}