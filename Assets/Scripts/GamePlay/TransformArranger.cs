using UnityEngine;

public class TransformArranger : MonoBehaviour
{
    [SerializeField] private Transform slot1;
    [SerializeField] private Transform slot2;
    [SerializeField] private Transform slot3;

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
        slot1.localPosition = new Vector3(0f, -8.5f, 0f);
        slot2.localPosition = new Vector3(1f, -8.5f, 0f);
        slot3.localPosition = new Vector3(2f, -8.5f, 0f);
    }
}
