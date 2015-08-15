using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos
{
    public class HitTrigger : MonoBehaviour
    {
        [SerializeField]
        HitReaction hitReaction;
        [SerializeField]
        float hitForce = 1f;

        bool isReact;
        Vector3 reactVelocity;
        public float forceMlp = 1f; // Explosion force
        public float upForce = 1f; // Explosion up forve
        public float weightFalloffSpeed = 1f; // The speed of explosion falloff

        public GameObject target;
        Rigidbody targetRigidbody;

        private string colliderName;

        void Start()
        {
            //targetRigidbody = target.gameObject.GetComponent<Rigidbody>();

        }

        void Update()
        {
            if (isReact)
            {

            }
        }

        void OnGUI()
        {
            GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
            if (colliderName != string.Empty) GUILayout.Label("Last Bone Hit: " + colliderName);
        }
        //public void OnTriggerEnter(Collider collision)
        public void OnCollisionEnter(Collision collision)
        {
            //if (!isReact)
            //    return;
            Collider collider = collision.gameObject.GetComponent<Collider>();
            Vector3 dir = collision.relativeVelocity;

            ContactPoint contact = collision.contacts[0];
            Vector3 point = contact.point;
            // Use the HitReaction
            hitReaction.Hit(collider, -dir * hitForce, point);

            isReact = true;
            reactVelocity = dir;
            // root transform
            Vector3 direction = -dir * hitForce;
            direction.y = 0;
            //target.transform.Translate(move);

            MonsterController monsterController = collision.transform.GetComponentInParent<MonsterController>();
            if (monsterController != null)
            {
                target = monsterController.gameObject;
                targetRigidbody = target.GetComponent<Rigidbody>();

                //float explosionForce = explosionForceByDistance.Evaluate(direction.magnitude);
                targetRigidbody.velocity = (direction.normalized + (Vector3.up * upForce)) * 1 * forceMlp;
            }

            // Just for GUI
            colliderName = collider.name;

            Debug.Log(gameObject.name + "-OnCollisionEnter: " + collision.gameObject.name);
        }

    }
}