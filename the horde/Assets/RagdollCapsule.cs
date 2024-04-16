using UnityEngine;
using System.Collections.Generic;

public class RagdollCapsule : MonoBehaviour
{
    public static readonly HashSet<RagdollCapsule> allUnconnectedRagdolls = new HashSet<RagdollCapsule>();

    [SerializeField]
    PooledExplosion explosion = null;
    [SerializeField]
    List<StickyRigidbody> allStickies = new List<StickyRigidbody>();
    [SerializeField]
    float drag = 1f;
	[SerializeField]
	StickyRigidbody connectedBody = null;
    //[SerializeField]
    //float appliedForce = 10f;

    bool collided = false;
    int index;
    Vector3 forceCache;

    public PooledExplosion Explosion
    {
        get
        {
            return explosion;
        }
    }

    void Start()
    {
        if (allUnconnectedRagdolls.Contains(this) == false)
        {
            allUnconnectedRagdolls.Add(this);
        }
    }

    //void FixedUpdate()
    //{
    //    if(collided == true)
    //    {
    //        // Grab the camera's direction, and calculate force
    //        forceCache = Camera.main.transform.forward * appliedForce;

    //        // Go through all stickes
    //        for(index = 0; index < allStickies.Count; ++index)
    //        {
    //            if(allStickies[index] != connectedBody)
    //            {
    //                allStickies[index].Body.AddForce(forceCache, ForceMode.Acceleration);
    //            }
    //        }
    //    }
    //}

    void OnDestroy()
    {
        if (allUnconnectedRagdolls.Contains(this) == true)
        {
            allUnconnectedRagdolls.Remove(this);
        }
    }

    public void CollidedWithSticky(StickyRigidbody collidedWith)
    {
        if(collided == false)
        {
            // Remove everything
            Destroy(GetComponent<Animator>());

            // Make all the scripts floppy
            foreach(StickyRigidbody script in allStickies)
            {
                script.Connect();
            }

            // Remove this ragdoll from the list
            OnDestroy();

            connectedBody = collidedWith;
            collided = true;
        }
    }

    [ContextMenu("Setup")]
    public void SetupRagdoll()
    {
        Rigidbody[] allBodies = GetComponentsInChildren<Rigidbody>();
        allStickies.Clear();

        foreach(Rigidbody body in allBodies)
        {
            if(body.gameObject != gameObject)
            {
                body.isKinematic = false;
                body.drag = drag;

                StickyRigidbody script = body.GetComponent<StickyRigidbody>();
                if(script == null)
                {
                    script = body.gameObject.AddComponent<StickyRigidbody>();
                }
                script.Parent = this;

                allStickies.Add(script);
            }
        }
    }
}
