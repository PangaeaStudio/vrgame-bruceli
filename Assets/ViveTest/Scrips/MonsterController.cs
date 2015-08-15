using UnityEngine;
using System.Collections;
using RootMotion.Demos;

public class MonsterController : MonoBehaviour
{
    HitReaction hitReaction;
    Rigidbody rigidbody;

    void Awake()
    {
        hitReaction = GetComponent<HitReaction>();

        rigidbody = GetComponent<Rigidbody>();
    }

	public void Hit(Collider collider, Vector3 direction, Vector3 point, int damage)
    {
        // Use the HitReaction
        hitReaction.Hit(collider, direction, point);

        direction.y = 0;
        rigidbody.velocity = (direction.normalized + (Vector3.up * 1)) * 1 * 1;

    }

    public void Grab(Vector3 direction)
    {
    }

    public void Drop()
    {
    	
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
