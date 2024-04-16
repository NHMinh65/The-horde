using UnityEngine;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    public bool IsVisible
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void Hide()
    {
        // Make the game object inactive
        gameObject.SetActive(false);
    }

    public void OnStartClicked()
    {
        // Hide the panel
        Hide();
    }
}
