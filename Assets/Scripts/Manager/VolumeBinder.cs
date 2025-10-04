using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeBinder : MonoBehaviour
{
    public enum Kind { Master, BGM, SFX }
    public Kind kind;

    [Header("Optional UI")]
    public TextMeshProUGUI valueText;

    private Slider slider;
    private bool ready = false;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void OnEnable()
    {
        // PlayerPrefs �� �ε�
        float saved = kind switch
        {
            Kind.Master => PlayerPrefs.GetFloat("vol_master", 1f),
            Kind.BGM => PlayerPrefs.GetFloat("vol_bgm", 1f),
            Kind.SFX => PlayerPrefs.GetFloat("vol_sfx", 1f),
            _ => 1f
        };

        // �����̴� �ʱ�ȭ �� �̺�Ʈ�� ��� �ߵ����� �ʰ� ����
        slider.onValueChanged.RemoveAllListeners();
        slider.value = saved;

        // �ؽ�Ʈ�� ��� �ݿ�
        UpdateValueText(saved);

        // MusicManager�� ����
        if (MusicManager.Instance != null)
        {
            switch (kind)
            {
                case Kind.Master:
                    slider.onValueChanged.AddListener(MusicManager.Instance.SetMasterVolume);
                    break;
                case Kind.BGM:
                    slider.onValueChanged.AddListener(MusicManager.Instance.SetBGMVolume);
                    break;
                case Kind.SFX:
                    slider.onValueChanged.AddListener(MusicManager.Instance.SetSFXVolume);
                    break;
            }
        }

        // ValueText ���ſ� ������ �߰�
        slider.onValueChanged.AddListener(UpdateValueText);
        ready = true;
    }

    void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveAllListeners();
        ready = false;
    }

    private void UpdateValueText(float v)
    {
        if (valueText == null) return;
        int percent = Mathf.RoundToInt(v * 100f);
        valueText.text = $"{percent}%";
    }
}
