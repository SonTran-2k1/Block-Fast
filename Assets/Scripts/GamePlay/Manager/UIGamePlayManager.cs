using Core.Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIGamePlayManager : SingletonBase<UIGamePlayManager>
{
    [Header("Score Display")] [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Combo Display")] [SerializeField] private Image comboProgressFill;
    [SerializeField] private Image comboImage; // Image nền combo
    [SerializeField] private Image XImage; // Image nền combo
    [SerializeField] private Image comboNumberUnit; // Image số hàng đơn vị (0-9)
    [SerializeField] private Image comboNumberTens; // Image số hàng chục (chỉ hiện khi >= 10)
    [SerializeField] private Sprite[] comboNumberSprites = new Sprite[10]; // Sprite số 0-9

    [Header("Animation Settings")] [SerializeField] private float comboScaleDuration = 0.2f;
    [SerializeField] private float comboScaleAmount = 1.3f; // Scale to 130%

    private Tween comboScaleTween;
    private Tween comboFadeTween;
    private Tween comboUnitScaleTween;
    private Tween comboTensScaleTween;

    private void Start()
    {
        // Subscribe vào events
        ScoreManager.Instance.ScoreChanged += UpdateScoreDisplay;
        ComboManager.Instance.ComboChanged += OnComboChanged;
        BestScoreManager.Instance.BestScoreUpdated += UpdateBestScoreDisplay;

        // Update initial values
        UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        UpdateBestScoreDisplay(BestScoreManager.Instance.BestScore);

        // Hide combo images at start
        if (comboImage != null)
        {
            comboImage.gameObject.SetActive(false);
        }

        if (comboNumberUnit != null)
        {
            comboNumberUnit.gameObject.SetActive(false);
        }

        if (comboNumberTens != null)
        {
            comboNumberTens.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Update combo progress bar mỗi frame
        if (comboProgressFill != null)
        {
            comboProgressFill.fillAmount = ComboManager.Instance.ComboProgress;
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void UpdateBestScoreDisplay(int newBestScore)
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {newBestScore}";
        }
    }

    private void OnComboChanged(int newCombo)
    {
        if (newCombo > 0)
        {
            // Show combo images
            ShowComboDisplay(true);

            // Kill previous tweens
            comboScaleTween?.Kill();
            comboFadeTween?.Kill();
            comboUnitScaleTween?.Kill();
            comboTensScaleTween?.Kill();

            // Reset colors
            ResetComboAlpha();

            // Update combo number sprites
            UpdateComboNumberDisplay(newCombo);

            // Scale animation cho combo image chính
            if (comboImage != null)
            {
                comboImage.transform.localScale = Vector3.one;
                comboScaleTween = comboImage.transform.DOScale(comboScaleAmount, comboScaleDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        if (comboImage != null)
                        {
                            comboScaleTween = comboImage.transform.DOScale(1f, comboScaleDuration * 0.5f).SetEase(Ease.InQuad);
                            comboScaleTween = XImage.transform.DOScale(1f, comboScaleDuration * 0.5f).SetEase(Ease.InQuad);
                        }
                    });
            }

            // Scale animation cho số
            if (comboNumberUnit != null)
            {
                comboNumberUnit.transform.localScale = Vector3.one;
                comboUnitScaleTween = comboNumberUnit.transform.DOScale(comboScaleAmount, comboScaleDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        if (comboNumberUnit != null)
                        {
                            comboUnitScaleTween = comboNumberUnit.transform.DOScale(1f, comboScaleDuration * 0.5f)
                                .SetEase(Ease.InQuad);
                        }
                    });
            }

            if (comboNumberTens != null && comboNumberTens.gameObject.activeSelf)
            {
                comboNumberTens.transform.localScale = Vector3.one;
                comboTensScaleTween = comboNumberTens.transform.DOScale(comboScaleAmount, comboScaleDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        if (comboNumberTens != null)
                        {
                            comboTensScaleTween = comboNumberTens.transform.DOScale(1f, comboScaleDuration * 0.5f)
                                .SetEase(Ease.InQuad);
                        }
                    });
            }

            Debug.Log($"[UIGamePlayManager] Combo {newCombo} - Scale animation!");
        }
        else if (newCombo == 0)
        {
            // Fade out và tắt combo images
            FadeOutComboDisplay();
        }
    }

    private void UpdateComboNumberDisplay(int combo)
    {
        if (combo < 10)
        {
            // Chỉ hiện số hàng đơn vị
            if (comboNumberUnit != null && combo < comboNumberSprites.Length && comboNumberSprites[combo] != null)
            {
                comboNumberUnit.sprite = comboNumberSprites[combo];
                comboNumberUnit.gameObject.SetActive(true);
            }

            // Ẩn số hàng chục
            if (comboNumberTens != null)
            {
                comboNumberTens.gameObject.SetActive(false);
            }
        }
        else
        {
            // Hiện cả 2 số (hàng chục và hàng đơn vị)
            int tens = combo / 10;
            int unit = combo % 10;

            if (comboNumberTens != null && tens < comboNumberSprites.Length && comboNumberSprites[tens] != null)
            {
                comboNumberTens.sprite = comboNumberSprites[tens];
                comboNumberTens.gameObject.SetActive(true);
            }

            if (comboNumberUnit != null && unit < comboNumberSprites.Length && comboNumberSprites[unit] != null)
            {
                comboNumberUnit.sprite = comboNumberSprites[unit];
                comboNumberUnit.gameObject.SetActive(true);
            }
        }
    }

    private void ShowComboDisplay(bool show)
    {
        if (comboImage != null)
        {
            comboImage.gameObject.SetActive(show);
        }
    }

    private void ResetComboAlpha()
    {
        if (comboImage != null) comboImage.color = Color.white;
        if (comboNumberUnit != null) comboNumberUnit.color = Color.white;
        if (comboNumberTens != null) comboNumberTens.color = Color.white;
    }

    private void FadeOutComboDisplay()
    {
        if (comboImage == null || !comboImage.gameObject.activeSelf) return;

        comboScaleTween?.Kill();
        comboFadeTween?.Kill();
        comboUnitScaleTween?.Kill();
        comboTensScaleTween?.Kill();

        // Fade out tất cả combo images
        Sequence fadeSequence = DOTween.Sequence();

        if (comboImage != null)
        {
            fadeSequence.Join(comboImage.DOFade(0, 0.5f));
            fadeSequence.Join(XImage.DOFade(0, 0.5f));
        }

        if (comboNumberUnit != null && comboNumberUnit.gameObject.activeSelf)
        {
            fadeSequence.Join(comboNumberUnit.DOFade(0, 0.5f));
        }

        if (comboNumberTens != null && comboNumberTens.gameObject.activeSelf)
        {
            fadeSequence.Join(comboNumberTens.DOFade(0, 0.5f));
        }

        fadeSequence.OnComplete(() =>
        {
            ShowComboDisplay(false);
            if (comboNumberUnit != null) comboNumberUnit.gameObject.SetActive(false);
            if (comboNumberTens != null) comboNumberTens.gameObject.SetActive(false);
            ResetComboAlpha();
        });

        comboFadeTween = fadeSequence;

        Debug.Log("[UIGamePlayManager] Combo reset - Fade out!");
    }

    private void OnDestroy()
    {
        // Kill all tweens
        comboScaleTween?.Kill();
        comboFadeTween?.Kill();
        comboUnitScaleTween?.Kill();
        comboTensScaleTween?.Kill();

        // Unsubscribe events
        if (ScoreManager.IsInitialized)
        {
            ScoreManager.Instance.ScoreChanged -= UpdateScoreDisplay;
        }

        if (ComboManager.IsInitialized)
        {
            ComboManager.Instance.ComboChanged -= OnComboChanged;
        }

        if (BestScoreManager.IsInitialized)
        {
            BestScoreManager.Instance.BestScoreUpdated -= UpdateBestScoreDisplay;
        }
    }
}
