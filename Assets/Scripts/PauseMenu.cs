using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Slider qualitySlider;

    private bool isPaused = false;

    void Start()
    {
        // Set the slider's range and initial value
        qualitySlider.minValue = 0;
        qualitySlider.maxValue = QualitySettings.names.Length - 1;
        qualitySlider.value = QualitySettings.GetQualityLevel();
        qualitySlider.wholeNumbers = true; // Ensure the slider only allows whole numbers

        // Add listener for when the slider value changes
        qualitySlider.onValueChanged.AddListener(delegate {
            ChangeQualityLevel((int)qualitySlider.value);
        });

        // Ensure the pause menu is hidden at the start
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Toggle pause menu on pressing the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void ChangeQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pause game time
        isPaused = true;
    }
} 