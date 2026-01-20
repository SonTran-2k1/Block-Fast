using UnityEngine;

public class TransformArranger : MonoBehaviour
{
    [SerializeField] private Transform slot1;
    [SerializeField] private Transform slot2;
    [SerializeField] private Transform slot3;
    [SerializeField] private float yPosition = -8.5f;
    [SerializeField] private float xSpacing = 1f;

    private void OnEnable()
    {
        ArrangeSlots();
    }

    public void ArrangeSlots()
    {
        if (slot1 == null || slot2 == null || slot3 == null)
        {
            Debug.LogError("[TransformArranger] Not all slots are assigned");
            return;
        }

        // Arrange with y = -8.5 and x spacing = 1
        slot1.localPosition = new Vector3(0f - xSpacing, yPosition, 0f);
        slot2.localPosition = new Vector3(0f, yPosition, 0f);
        slot3.localPosition = new Vector3(0f + xSpacing, yPosition, 0f);
    }
}
