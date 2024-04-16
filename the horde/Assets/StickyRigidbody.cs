using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class StickyRigidbody : MonoBehaviour
{
    const string StickyTag = "Sticky";

    [SerializeField]
    RagdollCapsule parent = null;
	//[SerializeField]
	const float springStrength = 10000f;
	//[SerializeField]
	static readonly Vector2 springDistance = new Vector2(0f, 0.1f);

    bool connected = false;
    Rigidbody body;

    public RagdollCapsule Parent
    {
        set
        {
            parent = value;
        }
    }

    public bool IsConnected
    {
        get
        {
            return connected;
        }
    }

    public Rigidbody Body
    {
        get
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }
            return body;
        }
    }

    public void Connect()
    {
        if(IsConnected == false)
        {
            // Stop making this kinematic
            Body.isKinematic = false;

            // Mark this as sticky
            gameObject.tag = StickyTag;

            // Indicate we're connected
            connected = true;
        }
    }

    void OnCollisionEnter(Collision info)
    {
        // Check tag
        if ((IsConnected == false) && (info.collider.CompareTag(StickyTag) == true))
        {
            // Check if there's a rigid body
            Rigidbody otherBody = info.collider.GetComponent<Rigidbody>();
            if(otherBody != null)
            {
                // Create a Fixed Joint
                //FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                SpringJoint joint = gameObject.AddComponent<SpringJoint>();
                joint.connectedBody = otherBody;
				joint.spring = springStrength;
				//joint.minDistance = springDistance.x;
				//joint.maxDistance = springDistance.y;

                // Notify the parent
                parent.CollidedWithSticky(this);

                // Play an animation
                Singleton.Get<PoolingManager>().GetInstance(parent.Explosion.gameObject, info.contacts[0].point, Quaternion.identity);
            }
        }
    }
}
