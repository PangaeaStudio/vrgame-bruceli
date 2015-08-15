using UnityEngine;
using System.Collections.Generic;
using RootMotion.Demos;
using NodeCanvas.Framework;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    GameObject targetPlayer;
    [SerializeField]
    GameObject[] wayPoints;

    HitReaction hitReaction;
    Rigidbody rigidbody;

    Blackboard blackBoard;

    void Awake()
    {
        hitReaction = GetComponent<HitReaction>();

        rigidbody = GetComponent<Rigidbody>();

        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindGameObjectWithTag("Player");
        }

        if (wayPoints.Length == 0)
        {
            wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
        }

        if(blackBoard == null)
            blackBoard = gameObject.GetComponent<Blackboard>();

        blackBoard.SetValue("target", targetPlayer);

        List<GameObject> wps = new List<GameObject>(wayPoints);
        blackBoard.SetValue("PatrolWayPoints", wps);
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
