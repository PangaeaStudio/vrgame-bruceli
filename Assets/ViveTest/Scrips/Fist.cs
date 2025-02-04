﻿using UnityEngine;
using RootMotion.Demos;

public class Fist : HitTrigger 
{
    private FixedJoint joint;
    private Collider lastCollider;

    protected override void ProcessWeaponCollision(Collider collider, Vector3 dir, Vector3 hitPoint)
    {

        base.ProcessWeaponCollision(collider, dir, hitPoint);
        if(joint == null)
        {
            lastCollider = collider;
        }
    }

    private bool IsPressDown()
    {
        if(input == null)
            return false;
        return input.GetTriger() >= 1;
    }

    private bool IsPressUP()
    {
        if(input == null)
            return false;
        return input.GetTriger() <= 0;
    }

    protected override void OnUpdate()
    {
        if(IsPressDown() && joint == null)
        {
            if(lastCollider == null)
                return;
            
            Rigidbody targetRig = lastCollider.gameObject.GetComponent<Rigidbody>();
            
            if(targetRig != null )
            {
                MonsterController monsterController = lastCollider.transform.GetComponentInParent<MonsterController>();
                if(monsterController != null)
                {
                    joint = gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = targetRig;
                    monsterController.Grab(Vector3.zero);
                }
            }
        }
        else if (joint != null && IsPressUP())
        {
            
            MonsterController monsterController = joint.transform.root.GetComponent<MonsterController>();
            DestroyImmediate(joint);
            if(monsterController != null)
            {
                monsterController.Drop();
            }
            joint = null;
        }
    }
}
