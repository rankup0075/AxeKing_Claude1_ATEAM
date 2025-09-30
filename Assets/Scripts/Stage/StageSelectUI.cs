// StageSelectUI.cs  [�ֽź�]
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    public static StageSelectUI Instance;

    [Header("Root")]
    public GameObject stageSelectPanel;

    [Header("Views")]
    public GameObject regionView;
    public GameObject regionDetailView;
    public GameObject stageView;

    [Header("Region UI")]
    public Transform regionContent;        // RegionView/Viewport/Content
    public GameObject regionButtonPrefab;  // RegionButtonUI ���� ������

    [Header("Region Detail UI")]
    public TextMeshProUGUI regionTitleText;
    public TextMeshProUGUI regionDescText;
    public Transform rewardGroup;
    public Button enterButton;

    [Header("Stage UI")]
    public Transform stageContent;         // StageView/Viewport/Content
    public GameObject stageButtonPrefab;   // StageButtonUI ���� ������

    [Header("Extra Buttons")]
    public Button returnToTownButton;

    private RegionData currentRegion;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
        if (regionView != null) regionView.SetActive(false);
        if (regionDetailView != null) regionDetailView.SetActive(false);
        if (stageView != null) stageView.SetActive(false);
    }

    void Update()
    {
        if (!stageSelectPanel.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscape();
    }

    // ===== Open/Close =====
    public void Open()
    {
        stageSelectPanel.SetActive(true);

        regionView.SetActive(true);
        regionDetailView.SetActive(false);
        stageView.SetActive(false);

        PopulateRegions();

        if (returnToTownButton != null)
        {
            // [����] ���� ���� ���� ��ư ���̱�/�����
            string currentScene = SceneManager.GetActiveScene().name;
            bool isStageScene = currentScene.StartsWith("Stage");
            returnToTownButton.gameObject.SetActive(isStageScene);

            returnToTownButton.onClick.RemoveAllListeners();
            returnToTownButton.onClick.AddListener(() =>
            {
                UIManager.Instance.CloseStageSelectPanel();
                StageManager.Instance.ReturnToTown();
            });
        }
    }

    public void Close()
    {
        UIManager.Instance?.CloseStageSelectPanel();
    }

    // ESC ó���� ������ �ܰ迡�� ����
    public void HandleEscape()
    {
        if (stageView.activeSelf)
        { 
            stageView.SetActive(false); 
            regionView.SetActive(true); 
            return; 
        }
        if (regionDetailView.activeSelf)
        { 
            regionDetailView.SetActive(false); 
            OpenRegionView();
            return; 
        }
        UIManager.Instance?.CloseStageSelectPanel();
    }

    // ===== View ��ȯ =====
    private void OpenRegionView()
    {
        regionView.SetActive(true);
        regionDetailView.SetActive(false);
        stageView.SetActive(false);
        PopulateRegions();
    }

    private void OpenRegionDetailView(RegionData region)
    {
        currentRegion = region;

        regionTitleText.text = region.regionName;
        regionDescText.text = region.regionDescription;

        foreach (Transform c in rewardGroup) Destroy(c.gameObject);
        foreach (var r in region.rewards)
        {
            var go = new GameObject("RewardIcon", typeof(Image));
            go.transform.SetParent(rewardGroup, false);
            go.GetComponent<Image>().sprite = r.icon;
        }

        regionView.SetActive(false);
        regionDetailView.SetActive(true);
        stageView.SetActive(false);

        enterButton.onClick.RemoveAllListeners();
        enterButton.onClick.AddListener(() => OpenStageView(currentRegion));
    }

    private void OpenStageView(RegionData region)
    {
        foreach (Transform c in stageContent) Destroy(c.gameObject);

        foreach (var stage in region.stages)
        {
            var go = Instantiate(stageButtonPrefab, stageContent);
            var ui = go.GetComponent<StageButtonUI>();
            if (ui != null) ui.Setup(stage);     // ���⼭ lockOverlay/Interactable �ݿ�
        }

        regionView.SetActive(false);
        regionDetailView.SetActive(false);
        stageView.SetActive(true);
    }

    // ===== ��ư ���� =====
    private void PopulateRegions()
    {
        foreach (Transform c in regionContent) Destroy(c.gameObject);

        foreach (var region in StageManager.Instance.regions)
        {
            var go = Instantiate(regionButtonPrefab, regionContent);
            var ui = go.GetComponent<RegionButtonUI>();
            if (ui != null)
            {
                ui.Setup(region); // ���ǥ��/Interactable �ݿ�
                ui.AddClickListener(() => OpenRegionDetailView(region));
            }
        }
    }
}
