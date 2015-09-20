using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using Pangaea.Input;

namespace RootMotion.Demos
{
    public class HitTrigger : MonoBehaviour
    {
        [SerializeField]
        float hitForce = 1f;
        [SerializeField]
        int damage = 1;

        bool isReact;
        Vector3 reactVelocity;
        public float forceMlp = 1f; // Explosion force
        public float upForce = 1f; // Explosion up forve
        public float weightFalloffSpeed = 1f; // The speed of explosion falloff

        public GameObject target;

        private string colliderName;

        public bool IsUsingJoint = false;

        public GameObject[] effects;

        protected IPanInputDevice input;

        void Start()
        {

        }

        void Update()
        {
            if (isReact)
            {

            }
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {

        }

        void OnGUI()
        {
            GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
            if (colliderName != string.Empty) GUILayout.Label("Last Bone Hit: " + colliderName);
        }
        public void OnTriggerEnter(Collider collider)
        {
            Vector3 dir = collider.gameObject.transform.position - transform.position;

            Vector3 point = collider.gameObject.transform.position;

            colliderName = collider.name;

            ProcessWeaponCollision(collider, -dir, point);

        }
        public void OnCollisionEnter(Collision collision)
        {
            //if (!isReact)
            //    return;
            Collider collider = collision.gameObject.GetComponent<Collider>();
            Vector3 dir = GetVelocity();

            ContactPoint contact = collision.contacts[0];
            Vector3 point = contact.point;

            ProcessWeaponCollision(collider, dir, point);

            // // Just for GUI
            colliderName = collider.name;

            // Debug.Log(gameObject.name + "-OnCollisionEnter: " + collision.gameObject.name);

        }

        protected virtual void ProcessWeaponCollision(Collider collider, Vector3 dir, Vector3 hitPoint)
        {
            Vector3 direction = -dir * hitForce;

            MonsterController monsterController = collider.transform.GetComponentInParent<MonsterController>();
            if (monsterController != null)
            {
                target = monsterController.gameObject;
                // targetRigidbody = target.GetComponent<Rigidbody>();
                monsterController.Hit(collider,direction, hitPoint, damage);
            }


            int effectNum;
            if (effects == null)
            {
                effectNum = 0;
            }
            else
            {
                effectNum = effects.Length;
            }
            if (effectNum > 0)
            {
                int eindex = Random.Range(0, effectNum);
                GameObject go = (GameObject)GameObject.Instantiate(effects[eindex], hitPoint, Quaternion.identity);
                Object.Destroy(go, 1);
            }
        }

        public void OnPickUp(IPanInputDevice input, FixedJoint joint)
        {
            this.input = input;
            joint.connectedBody = transform.GetComponent<Rigidbody>();
         
        }

        public void OnDrop(IPanInputDevice input, FixedJoint joint)
        {
            joint.connectedBody = null;
            GameObject.DestroyImmediate(gameObject);
        }

        private Vector3 GetVelocity()
        {   
            return (input == null)? Vector3.zero : input.GetVelocity();
        }

    }
}