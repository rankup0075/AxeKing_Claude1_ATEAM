using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RewardData
{
    public string rewardName;
    public Sprite icon;
}

[System.Serializable]
public class RegionData
{
    public string regionId;         // "Region1"
    public string regionName;       // ex: "�׶� �ձ� �ܰ�"
    public string regionDescription;// �� ����

    public List<RewardData> rewards; // ���� ���
    public List<StageData> stages;   // �� ������ ���Ե� ����������

    public Sprite thumbnail;     // ��ư ����� �̹���
    public bool isUnlocked = false; // ��� ���� ���� (�⺻�� false)
   // public bool isCompleted = false; // Ŭ���� ����
}
