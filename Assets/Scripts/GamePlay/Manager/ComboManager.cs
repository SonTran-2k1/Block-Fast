using Core.Managers;
using Core.Singleton;
using UnityEngine;

public class ComboManager : SingletonBase<ComboManager>
{
    private int currentCombo = 0;
    private const int MaxCombo = 9;
    private float comboResetTimer = 0f;
    private const float ComboResetDuration = 5f; // Thời gian combo tồn tại

    public int CurrentCombo => currentCombo;
    public float ComboProgress => comboResetTimer / ComboResetDuration;

    // Event khi combo thay đổi
    public delegate void OnComboChanged(int newCombo);

    public event OnComboChanged ComboChanged;

    private void Update()
    {
        // Giảm dần combo timer
        if (currentCombo > 0)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    // Tăng combo khi ăn được block
    public void AddCombo()
    {
        int previousCombo = currentCombo;
        currentCombo = Mathf.Min(currentCombo + 1, MaxCombo);
        comboResetTimer = ComboResetDuration;

        Debug.Log($"[ComboManager] Combo increased to {currentCombo}!");
        AudioManager.Instance.PlaySFX("Clear" + currentCombo);

        // Trigger event
        ComboChanged?.Invoke(currentCombo);
    }

    // Reset combo khi không ăn được
    public void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"[ComboManager] Combo reset from {currentCombo} to 0!");
            ComboChanged?.Invoke(0);
        }

        currentCombo = 0;
        comboResetTimer = 0;
    }

    // Lấy combo multiplier (1x, 2x, 3x, ... 9x)
    public int GetComboMultiplier()
    {
        return Mathf.Max(1, currentCombo);
    }
}
