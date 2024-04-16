using UnityEngine;
using System.Collections;

public class DieIfBelow : MonoBehaviour
{
    [SerializeField]
    RestartPanel deathPanel = null;
    [SerializeField]
    RestartPanel successPanel = null;
    [SerializeField]
    Transform playerPosition = null;
    [SerializeField]
    Transform deathPosition = null;

	// Update is called once per frame
	void Update ()
    {
        if ((deathPanel.IsVisible == false) && (successPanel.IsVisible == false))
        {
            if(RagdollCapsule.allUnconnectedRagdolls.Count <= 0)
            {
                successPanel.Show();
            }
            else if (playerPosition.position.y < deathPosition.position.y)
            {
                deathPanel.Show();
            }
        }
	}
}
