using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegionButtonUI : MonoBehaviour
{
    public Image regionImage;            // ����� (���û���)
    public TextMeshProUGUI regionNameText;
    public GameObject lockOverlay;       // ��� ǥ��
    public Button button;

    private RegionData regionData;

    public void Setup(RegionData region)
    {
        regionData = region;

        if (regionNameText != null)
            regionNameText.text = region.regionName;

        // ����� ������ ǥ��
        if (regionImage != null && region.thumbnail != null)
            regionImage.sprite = region.thumbnail;

        // ��� ó��
        bool unlocked = region.isUnlocked;
        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
        if (button != null) button.interactable = unlocked;
    }

    public void AddClickListener(UnityEngine.Events.UnityAction action)
{
    if (button != null)
    {
        button.onClick.RemoveListener(action); // �ߺ� ����
        button.onClick.AddListener(action);
    }
    else
    {
        Debug.LogWarning($"{name} ��ư ������ ������� �ʾҽ��ϴ�.");
    }
}
}
