using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : NetworkBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private bool _fromStartScreen;

    private int _numberOfRounds = 5;
    public int NumberOfRounds { get { return _numberOfRounds; } }
    private bool _usePowerups = true;
    public bool UsePowerups { get { return _usePowerups; } }

    [SerializeField] private Color _defaultBackgroundColour;
    public Color DefaultBackgroundColour { get { return _defaultBackgroundColour; } }
    [SerializeField] private Color _defaultPlayerOutlineColour;
    public Color DefaultPlayerOutlineColour { get { return _defaultPlayerOutlineColour; } }

    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Button _showColourPickerButton;
    [SerializeField] private Button _confirmColourButton;
    [SerializeField] private Button _resetColourButton;
    [SerializeField] private Button _showOutlineColourPickerButton;
    [SerializeField] private Button _confirmOutlineColourButton;
    [SerializeField] private Button _resetOutlineColourButton;
    [SerializeField] private FlexibleColorPicker _colourPicker;
    [SerializeField] private TMP_Text _uiScaleText;
    [SerializeField] private Slider _uiScaleSlider;
    [SerializeField] private TMP_Dropdown _player1UITypeDropdown;
    [SerializeField] private TMP_Dropdown _player2UITypeDropdown;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private GameObject _player1UITypeLayout;
    [SerializeField] private GameObject _player2UITypeLayout;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        SetupListeners();
        Hide();
    }

    public void SetRoundsPowerupsAndNames(int rounds, bool usePowerups, string player1Name, string player2Name)
    {
        _numberOfRounds = rounds;
        _usePowerups = usePowerups;
        PlayerManager.Instance.Players[0].Name = player1Name;
        PlayerManager.Instance.Players[1].Name = player2Name;
    }

    private void SetupListeners()
    {
        _settingsButton.onClick.AddListener(OnSettingsButton);
        _backButton.onClick.AddListener(OnBackButton);
        _quitButton.onClick.AddListener(OnQuitButton);

        _showColourPickerButton.onClick.AddListener(ShowBackgroundColourPicker);
        _confirmColourButton.onClick.AddListener(ConfirmBackgroundColour);
        _resetColourButton.onClick.AddListener(ResetBackgroundColour);

        _showOutlineColourPickerButton.onClick.AddListener(ShowPlayerOutlineColourPicker);
        _confirmOutlineColourButton.onClick.AddListener(ConfirmPlayerOutlineColour);
        _resetOutlineColourButton.onClick.AddListener(ResetPlayerOutlineColour);

        _player1UITypeDropdown.onValueChanged.AddListener(SetUIType1);
        _player2UITypeDropdown.onValueChanged.AddListener(SetUIType2);

        _musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        _sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        _uiScaleSlider.onValueChanged.AddListener(SetUIScale);
    }

    private void OnQuitButton()
    {
        Application.Quit();
    }

    private void OnSettingsButton()
    {
        Show();
    }

    public void OnSettingsButton(bool fromStartScreen)
    {
        _fromStartScreen = fromStartScreen;
        Show();
    }

    private void OnBackButton()
    {
        if (_fromStartScreen)
            UIManager.Instance.ShowHideUIElement(UIManager.Instance.StartScreenUI, true);

        _fromStartScreen = false;
        Hide();
    }

    public void Show()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.SettingsUI, true);

        _confirmColourButton.gameObject.SetActive(false);
        _confirmOutlineColourButton.gameObject.SetActive(false);

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _musicVolumeSlider.value = musicVolume;
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolumeSlider.value = sfxVolume;

        float defaultScale = 1f;
        _uiScaleSlider.value = PlayerPrefs.GetFloat("UIScale", defaultScale);
        _uiScaleText.text = (_uiScaleSlider.value * 100).ToString("F0") + "%";

        string uiTypeText = PlayerPrefs.GetString("UIType0", "Sliders");
        if (uiTypeText == "Sliders") _player1UITypeDropdown.value = 0;
        else _player1UITypeDropdown.value = 1;

        uiTypeText = PlayerPrefs.GetString("UIType1", "Sliders");
        if (uiTypeText == "Sliders") _player2UITypeDropdown.value = 0;
        else _player2UITypeDropdown.value = 1;

        // hide the layout of the player we are not (their layout is readonly values anyway)
        if (NetworkManager.Singleton.LocalClientId == 0)
            _player2UITypeLayout.SetActive(false);
        else
            _player1UITypeLayout.SetActive(false);
    }

    public void ResetBackgroundColour()
    {
        _colourPicker.SetColor(_defaultBackgroundColour);
        ConfirmBackgroundColour();
    }

    public void ShowBackgroundColourPicker()
    {
        string savedColourString = PlayerPrefs.GetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(_defaultBackgroundColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        _colourPicker.SetColor(savedColour);

        _colourPicker.gameObject.SetActive(true);
        _showColourPickerButton.gameObject.SetActive(false);
        _confirmColourButton.gameObject.SetActive(true);
        _resetColourButton.gameObject.SetActive(true);
    }

    public void ConfirmBackgroundColour()
    {
        Color colour = _colourPicker.color;

        Camera.main.backgroundColor = colour;
        PlayerPrefs.SetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(colour));
        _showColourPickerButton.gameObject.SetActive(true);
        _confirmColourButton.gameObject.SetActive(false);
        _colourPicker.gameObject.SetActive(false);
    }

    public void ResetPlayerOutlineColour()
    {
        _colourPicker.SetColor(_defaultPlayerOutlineColour);
        ConfirmPlayerOutlineColour();
    }

    public void ShowPlayerOutlineColourPicker()
    {
        string savedColourString = PlayerPrefs.GetString("PlayerOutlineColour", ColorUtility.ToHtmlStringRGBA(_defaultPlayerOutlineColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        _colourPicker.SetColor(savedColour);

        _colourPicker.gameObject.SetActive(true);
        _showOutlineColourPickerButton.gameObject.SetActive(false);
        _confirmOutlineColourButton.gameObject.SetActive(true);
        _resetOutlineColourButton.gameObject.SetActive(true);
    }

    public void ConfirmPlayerOutlineColour()
    {
        Color colour = _colourPicker.color;

        PlayerPrefs.SetString("PlayerOutlineColour", ColorUtility.ToHtmlStringRGBA(colour));
        _showOutlineColourPickerButton.gameObject.SetActive(true);
        _confirmOutlineColourButton.gameObject.SetActive(false);
        _colourPicker.gameObject.SetActive(false);

        if (PlayerManager.Instance.Players[0].PlayerGameObject != null)
        {
            Material mat = PlayerManager.Instance.Players[0].PlayerGameObject.GetComponentInChildren<SpriteRenderer>().material;
            mat.SetColor("_SolidOutline", colour);
            mat = PlayerManager.Instance.Players[1].PlayerGameObject.GetComponentInChildren<SpriteRenderer>().material;
            mat.SetColor("_SolidOutline", colour);
        }
    }

    public void SetUIScale(float scale)
    {
        float scaleText = scale * 100;
        _uiScaleText.text = scaleText.ToString("F0") + "%";
        PlayerPrefs.SetFloat("UIScale", scale);

        for (int i = 0; i < 2; i++)
        {
            if (PlayerManager.Instance.Players[i].PlayerUI)
                PlayerManager.Instance.Players[i].PlayerUI.transform.localScale = new Vector3(scale, scale, 0);
        }
    }

    public void SetUIType1(int type)
    {
        string typeString;

        if (type == 0) typeString = "Sliders";
        else typeString = "TextBoxes";

        if (PlayerManager.Instance.Players[0].PlayerUI != null)
            PlayerManager.Instance.Players[0].PlayerUI.UpdateViewMode(typeString);
    }

    public void SetUIType2(int type)
    {
        string typeString;

        if (type == 0) typeString = "Sliders";
        else typeString = "TextBoxes";

        if (PlayerManager.Instance.Players[1].PlayerUI != null)
            PlayerManager.Instance.Players[1].PlayerUI.UpdateViewMode(typeString);
    }

    private void Hide()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.SettingsUI, false);
    }

    public void SetNumberOfRounds(string numberOfRounds)
    {
        _numberOfRounds = int.Parse(numberOfRounds);
        PlayerPrefs.SetInt("NumberOfRounds", _numberOfRounds);
    }

    public void SetUsePowerupsToggle(bool usePowerups)
    {
        _usePowerups = usePowerups;
        if (usePowerups) PlayerPrefs.SetInt("UsePowerups", 1);
        else PlayerPrefs.SetInt("UsePowerups", 0);
    }

    private void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
}
