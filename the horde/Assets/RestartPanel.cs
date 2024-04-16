using UnityEngine;
using System.Collections;

public class RestartPanel : MonoBehaviour
{
    public bool IsVisible
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void Show()
    {
        // Make the game object inactive
        gameObject.SetActive(true);
    }

    public void OnRestartClicked()
    {
        // Reload this level
        Application.LoadLevel(Application.loadedLevel);
    }
}
