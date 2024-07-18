using UnityEngine;

public class BehaviorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BehaviorGraph behaviorGraph;
    [SerializeField] private GameObject pathMenu;
    [SerializeField] private GameObject traceMenu;
    [SerializeField] private GameObject traceDepthSubMenu;

    private void Awake()
    {
        SettingsMenu.OnBehaviorModeChange.AddListener(OnBehaviorModeChangedHandler);
        SettingsMenu.OnTraceModeChange.AddListener(OnTraceModeChangedHandler);
    }

    private void OnBehaviorModeChangedHandler(object arg0)
    {
        switch (SettingsMenu.SelectedBehaviorMode)
        {
            case SettingsMenu.BehaviorMode.Path:
                pathMenu.SetActive(true);
                traceMenu.SetActive(false);
                break;
            case SettingsMenu.BehaviorMode.Aggregation:
                pathMenu.SetActive(false);
                traceMenu.SetActive(false);
                break;
            case SettingsMenu.BehaviorMode.Trace:
                pathMenu.SetActive(false);
                traceMenu.SetActive(true);
                break;
            default:
                pathMenu.SetActive(false);
                traceMenu.SetActive(false);
                break;
        }
    }

    private void OnTraceModeChangedHandler(object arg0)
    {
        switch (SettingsMenu.SelectedTraceMode)
        {
            case SettingsMenu.TraceMode.Depth:
                traceDepthSubMenu.SetActive(true);
                break;
            case SettingsMenu.TraceMode.Explore:
                traceDepthSubMenu.SetActive(false);
                break;
        }
    }

    public void OnNextPath()
    {
        behaviorGraph.CommandIncreasePath();
    }

    public void OnPreviousPath()
    {
        behaviorGraph.CommandDecreasePath();
    }

    public void OnNextTrace()
    {
        behaviorGraph.CommandIncreaseTrace();
    }

    public void OnPreviousTrace()
    {
        behaviorGraph.CommandDecreaseTrace();
    }

    public void OnIncreaseTraceDepth()
    {
        behaviorGraph.CommandIncreaseTraceDepth();
    }

    public void OnDecreaseTraceDepth()
    {
        behaviorGraph.CommandDecreaseTraceDepth();
    }
}
