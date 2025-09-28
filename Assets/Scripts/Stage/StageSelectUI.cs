// StageSelectUI.cs  [최신본]
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject regionButtonPrefab;  // RegionButtonUI 포함 프리팹

    [Header("Region Detail UI")]
    public TextMeshProUGUI regionTitleText;
    public TextMeshProUGUI regionDescText;
    public Transform rewardGroup;
    public Button enterButton;

    [Header("Stage UI")]
    public Transform stageContent;         // StageView/Viewport/Content
    public GameObject stageButtonPrefab;   // StageButtonUI 포함 프리팹

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
    }

    //public void Close()
    //{
    //    // 루트까지 비활성. UIManager 쪽에서 다시 건드릴 필요 없음.
    //    stageView.SetActive(false);
    //    regionDetailView.SetActive(false);
    //    regionView.SetActive(false);
    //    stageSelectPanel.SetActive(false);
    //}

    //// ===== ESC 단계별 처리 =====
    //public void HandleEscape()
    //{
    //    if (stageView.activeSelf) { stageView.SetActive(false); regionDetailView.SetActive(true); return; }
    //    if (regionDetailView.activeSelf) { regionDetailView.SetActive(false); OpenRegionView(); return; }
    //    if (regionView.activeSelf) { Close(); return; }
    //    Close();
    //}
    public void Close()
    {
        UIManager.Instance?.CloseStageSelectPanel();
    }

    // ESC 처리의 마지막 단계에서 위임
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

    // ===== View 전환 =====
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
            if (ui != null) ui.Setup(stage);     // 여기서 lockOverlay/Interactable 반영
        }

        regionView.SetActive(false);
        regionDetailView.SetActive(false);
        stageView.SetActive(true);
    }

    // ===== 버튼 생성 =====
    private void PopulateRegions()
    {
        foreach (Transform c in regionContent) Destroy(c.gameObject);

        foreach (var region in StageManager.Instance.regions)
        {
            var go = Instantiate(regionButtonPrefab, regionContent);
            var ui = go.GetComponent<RegionButtonUI>();
            if (ui != null)
            {
                ui.Setup(region); // 잠금표시/Interactable 반영
                ui.AddClickListener(() => OpenRegionDetailView(region));
            }
        }
    }
}
