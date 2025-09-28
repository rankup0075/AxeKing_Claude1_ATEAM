using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageButtonUI : MonoBehaviour
{
    public Image stageImage;
    public TextMeshProUGUI stageNameText;
    public Button button;
    public GameObject lockOverlay; // Lock UI


    private StageData stageData;

    public void Setup(StageData data)
    {
        stageData = data;

        // �������� �̸� ǥ��
        if (stageNameText != null)
            stageNameText.text = stageData.stageName;

        // ����� ǥ��
        if (stageImage != null && stageData.thumbnail != null)
            stageImage.sprite = stageData.thumbnail;

        // ��� ���� �ݿ�
        if (lockOverlay != null)
            lockOverlay.SetActive(!stageData.isUnlocked);
        button.interactable = stageData.isUnlocked;

        // Ŭ�� �� ���� ����
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnStageSelected);


    }

    private void OnStageSelected()
    {
        Debug.Log($"�������� ���õ�: {stageData.stageName}");
        // UIManager�� StageManager ���ؼ� ���� �������� �ε�� ����

        if (stageData.isUnlocked)
        {
            // StageManager�� ���� �� �ε�
            StageManager.Instance.EnterStage(stageData.stageId);
        }
        else
        {
            Debug.LogWarning($"[StageButtonUI] {stageData.stageName} �� �������");
        }
    }

    //void UpdateUI()
    //{
    //    stageNameText.text = stageData.stageName;

    //    // ��� ���ο� ���� �������� ǥ��
    //    if (lockOverlay != null)
    //        lockOverlay.SetActive(!stageData.isUnlocked);

    //    // ��ư Ȱ��ȭ/��Ȱ��ȭ
    //    GetComponent<Button>().interactable = stageData.isUnlocked;
    //}
}
