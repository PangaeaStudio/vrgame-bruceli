using UnityEngine;
using System.Collections.Generic;
using RootMotion.Demos;
using NodeCanvas.Framework;
using NodeCanvas.BehaviourTrees;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    GameObject targetPlayer;
    [SerializeField]
    GameObject[] wayPoints;

    public int HP = 100;
    private int currentHP;

    HitReaction hitReaction;
    Rigidbody rigidbody;

    BehaviourTreeOwner behaviourTree;
    Blackboard blackBoard;

    Animator animator;

    public float HitReactTime = 1.0f;
    private bool isHitReact;
    private float hitRecoverTime;

    public float LightHitForceGrade;
    public float MediumHitForceGrade;
    public float HardHitForceGrade;

    private bool isDead;


    void Awake()
    {
        animator = GetComponent<Animator>();

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

        if (behaviourTree == null)
            behaviourTree = gameObject.GetComponent<BehaviourTreeOwner>();

        if (blackBoard != null)
            blackBoard.SetValue("target", targetPlayer);

        if (behaviourTree != null)
        {
            List<GameObject> wps = new List<GameObject>(wayPoints);
            blackBoard.SetValue("PatrolWayPoints", wps);
        }

        currentHP = HP;
    }

	public void Hit(Collider collider, Vector3 direction, Vector3 point, int damage)
    {
        if (isDead)
            return;

        if (isHitReact)
            return;

        currentHP -= damage;
        if (currentHP < 0)
            isDead = true;

        isHitReact = true;

        if(behaviourTree)
            behaviourTree.PauseBehaviour();

        float hitForce = direction.magnitude;
        //if (hitForce < LightHitForceGrade)
        {
            // Use the HitReaction
            hitReaction.Hit(collider, direction, point);
            animator.SetBool("IsHit", true);

            //direction.y = 0;
            ////rigidbody.velocity = (direction.normalized + (Vector3.up * 1)) * 1 * 1;
            //rigidbody.velocity = direction;
        }
        //else if (hitForce < MediumHitForceGrade)
        //{
        //    // Use the HitReaction
        //    hitReaction.Hit(collider, direction, point);
        //    animator.SetBool("IsHit", true);
        //}
        //else if (hitForce < HardHitForceGrade)
        //{
        //    animator.SetBool("IsKnockDown", true);
        //}
        //else
        //{
        //    //Knock Away
        //}
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
        if (isHitReact)
        {
            bool isHit = animator.GetBool("IsHit");
            bool isHitDown = animator.GetBool("IsKnockDown");

            //isHitReact = isHit | isHitDown;
            isHitReact = isHit;

            if (!isHitReact)
            {
                if (behaviourTree)
                    behaviourTree.StartBehaviour();
            }
        }
    }


}
