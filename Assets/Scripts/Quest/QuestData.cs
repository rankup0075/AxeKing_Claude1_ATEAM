[System.Serializable]
public class QuestData
{
    public string questId; //����Ʈ ���̵�
    public string questName; //����Ʈ �̸�
    public string description; //����

    public int targetProgress; //��ô��
    public int currentProgress; //���� ��ô��
    public long rewardGold; //���� ���

    public bool isAccepted; //�����ߴ���
    public bool isCompleted; //�Ϸ� �ߴ���

    public string prerequisiteQuestId; // ���� ����Ʈ ID
    public string requiredItemName; //�ʿ��� ������ �̸�
}
