using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private bool onBG, onEffects;
    [SerializeField] private TextMeshProUGUI textOnBG, textOffBG, textOnE, textOffE;
    [SerializeField] private AudioSource soundBG;
    [SerializeField] private AudioSource[] soundEffects;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;

        onBG = LoadSoundBG() == 1 ? true : false;
        onEffects = LoadSoundE() == 1 ? true : false;
        SaveText(textOnBG, textOffBG, onBG);
        SaveText(textOnE, textOffE, onEffects);
        SoundSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame() => SceneManager.LoadScene(0);
    public void PlayGame() => SceneManager.LoadScene(1);
    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void PauseGame() => Time.timeScale = 0f;
    public void Resume() => Time.timeScale = 1f;

    public void ClickSoundBG()
    {
        onBG = !onBG;
        PlayerPrefs.SetInt("soundBGPref", onBG ? 1 : 0);
        SaveText(textOnBG, textOffBG, onBG);
        SoundSettings();
    }

    public void ClickSoundE()
    {
        onEffects = !onEffects;
        PlayerPrefs.SetInt("soundEPref", onEffects ? 1 : 0);
        SaveText(textOnE, textOffE, onEffects);
        SoundSettings();
    }

    private void SoundSettings()
    {
        soundBG.volume = LoadSoundBG();
        foreach (var item in soundEffects)
            item.volume = LoadSoundE();
    }

    private void SaveText(TextMeshProUGUI textOn, TextMeshProUGUI textOff, bool state)
    {
        if (textOn && textOff)
        {
            textOn.gameObject.SetActive(state);
            textOff.gameObject.SetActive(!state);
        }
    }

    private int LoadSoundBG() => PlayerPrefs.GetInt("soundBGPref", 1);
    private int LoadSoundE() => PlayerPrefs.GetInt("soundEPref", 1);
}
