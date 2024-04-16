using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    int recoverHealth = 5;

    // This method could be invoked when the asteroid is destroyed
    public void Die()
    {
        // Implement the functionality of the Die method
        // Example: Add recoverHealth to the ship's health
        ShipControl.Instance.CurrentHealth += recoverHealth;

        // Optionally, destroy the asteroid object
        Destroy(gameObject);
    }
}
