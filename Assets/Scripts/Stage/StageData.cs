using UnityEngine;

[System.Serializable]
public class StageData
{
    public string stageId;       // "1-1", "1-2" ����
    public string stageName;     // "�������� 1"
    public string sceneName;     // ���� �� �̸�

    [Header("UI Settings")]
    public Sprite thumbnail;     // ��ư ����� �̹���
    public bool isUnlocked = false; // ��� ���� ���� (�⺻�� false)
    public bool isCompleted = false; // Ŭ���� ����
}
