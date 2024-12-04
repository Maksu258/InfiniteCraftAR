using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeMenuManager : MonoBehaviour
{
    // Called when the Start button is clicked
    public void OnStartButtonClicked()
    {
        // Load the AR scene
        SceneManager.LoadScene("SampleScene");
    }

    // Called when the Reset button is clicked
    public void OnResetButtonClicked()
    {
        // Clear all saved data
        Debug.Log("Progress reset successfully.");
        PlayerPrefs.DeleteAll();

      
    }
}
