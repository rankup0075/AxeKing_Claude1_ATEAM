using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;

    [Header("Visual Settings")]
    public bool showNumbers = true;
    public bool showName = false;
    public bool smoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;
    public float midHealthThreshold = 0.6f;

    [Header("Auto Hide")]
    public bool autoHide = false;
    public float hideDelay = 3f;

    private int currentHealth;
    private int maxHealth;
    private float targetSliderValue;
    private bool isVisible = true;
    private Coroutine hideCoroutine;

    void Start()
    {
        InitializeHealthBar();
    }

    void InitializeHealthBar()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (fillImage == null && healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        // 초기 설정
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            targetSliderValue = 1f;
        }

        // 텍스트 표시 설정
        if (healthText != null)
            healthText.gameObject.SetActive(showNumbers);

        if (nameText != null)
            nameText.gameObject.SetActive(showName);
    }

    void Update()
    {
        // 부드러운 전환 처리
        if (smoothTransition && healthSlider != null)
        {
            if (Mathf.Abs(healthSlider.value - targetSliderValue) > 0.01f)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetSliderValue, transitionSpeed * Time.deltaTime);
                UpdateHealthColor();
            }
        }
    }

    public void UpdateHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;

        if (maxHealth <= 0) return;

        float healthRatio = (float)current / max;
        targetSliderValue = healthRatio;

        if (!smoothTransition && healthSlider != null)
        {
            healthSlider.value = healthRatio;
            UpdateHealthColor();
        }

        // 텍스트 업데이트
        if (healthText != null && showNumbers)
        {
            healthText.text = $"{current}/{max}";
        }

        // 자동 숨김 처리
        if (autoHide)
        {
            ShowHealthBar();

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            if (current < max) // 체력이 최대가 아닐 때만 자동 숨김
                hideCoroutine = StartCoroutine(AutoHideCoroutine());
        }
    }

    void UpdateHealthColor()
    {
        if (fillImage == null) return;

        float healthRatio = healthSlider.value;

        if (healthRatio <= lowHealthThreshold)
        {
            fillImage.color = lowHealthColor;
        }
        else if (healthRatio <= midHealthThreshold)
        {
            float t = (healthRatio - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold);
            fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else
        {
            float t = (healthRatio - midHealthThreshold) / (1f - midHealthThreshold);
            fillImage.color = Color.Lerp(midHealthColor, fullHealthColor, t);
        }
    }

    public void SetCharacterName(string characterName)
    {
        if (nameText != null)
        {
            nameText.text = characterName;
            nameText.gameObject.SetActive(showName && !string.IsNullOrEmpty(characterName));
        }
    }

    public void ShowHealthBar()
    {
        if (!isVisible)
        {
            isVisible = true;
            gameObject.SetActive(true);

            // 페이드 인 애니메이션 (선택사항)
            if (GetComponent<CanvasGroup>() != null)
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    public void HideHealthBar()
    {
        if (isVisible)
        {
            isVisible = false;

            // 페이드 아웃 애니메이션 (선택사항)
            if (GetComponent<CanvasGroup>() != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    System.Collections.IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(hideDelay);
        HideHealthBar();
    }

    System.Collections.IEnumerator FadeIn()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        float fadeTime = 0.3f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    System.Collections.IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float fadeTime = 0.3f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // 외부에서 설정 가능한 메서드들
    public void SetShowNumbers(bool show)
    {
        showNumbers = show;
        if (healthText != null)
            healthText.gameObject.SetActive(show);
    }

    public void SetShowName(bool show)
    {
        showName = show;
        if (nameText != null)
            nameText.gameObject.SetActive(show && !string.IsNullOrEmpty(nameText.text));
    }

    public void SetAutoHide(bool autoHideEnabled, float delay = 3f)
    {
        autoHide = autoHideEnabled;
        hideDelay = delay;
    }

    // 현재 상태 확인용
    public float GetHealthRatio()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    public bool IsVisible => isVisible;
}