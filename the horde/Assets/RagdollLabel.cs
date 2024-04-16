using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class RagdollLabel : MonoBehaviour
{
    Text cache;

	// Use this for initialization
	void Start ()
    {
        cache = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        cache.text = RagdollCapsule.allUnconnectedRagdolls.Count.ToString();
	}
}
