using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class SettingsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject settingPrefab;
    [SerializeField] private GameObject selectableSettingPrefab;
    [SerializeField] private RectTransform background;

    //############################//
    //                            //
    //       Relation Type        //
    //                            //
    //############################//

    public enum ClassDiagramRelationType
    {
        //Contains,
        //Contructs,
        //Holds,
        Calls,
        //Accepts,
        Specializes,
        //Returns,
        //Accesses,
    }

    public static UnityEvent<object> OnRelationTypeChange = new UnityEvent<object>();

    private static ClassDiagramRelationType _selectedRelationType;

    public static ClassDiagramRelationType SelectedRelationType
    {
        get => _selectedRelationType;
    }

    public static void LoadRelationType()
    {
        ClassDiagramRelationType selectedRelationType = (ClassDiagramRelationType)PlayerPrefs.GetInt(nameof(ClassDiagramRelationType), (int)ClassDiagramRelationType.Specializes);
        _selectedRelationType = selectedRelationType;
        OnRelationTypeChange.Invoke(selectedRelationType);
    }

    public static void SetRelationType(object newRelationType)
    {
        ClassDiagramRelationType relationType = (ClassDiagramRelationType)newRelationType;
        _selectedRelationType = relationType;
        PlayerPrefs.SetInt(nameof(ClassDiagramRelationType), (int)relationType);
        OnRelationTypeChange.Invoke(relationType);
    }

    //############################//
    //                            //
    //       Behavior Mode        //
    //                            //
    //############################//

    public enum BehaviorMode
    {
        Aggregation,
        Path,
        Trace,
    }

    public static UnityEvent<object> OnBehaviorModeChange = new UnityEvent<object>();

    private static BehaviorMode _selectedBehaviorMode;

    public static BehaviorMode SelectedBehaviorMode
    {
        get => _selectedBehaviorMode;
    }

    public static void LoadBehaviorMode()
    {
        BehaviorMode selectedBehaviorMode = (BehaviorMode)PlayerPrefs.GetInt(nameof(BehaviorMode), (int)BehaviorMode.Aggregation);
        _selectedBehaviorMode = selectedBehaviorMode;
        OnBehaviorModeChange.Invoke(selectedBehaviorMode);
    }

    public static void SetBehaviorMode(object newBehaviorMode)
    {
        BehaviorMode behaviorMode = (BehaviorMode)newBehaviorMode;
        _selectedBehaviorMode = behaviorMode;
        PlayerPrefs.SetInt(nameof(BehaviorMode), (int)behaviorMode);
        OnBehaviorModeChange.Invoke(behaviorMode);
    }

    //############################//
    //                            //
    //         Trace Mode         //
    //                            //
    //############################//

    public enum TraceMode
    {
        Depth,
        Explore,
    }

    public static UnityEvent<object> OnTraceModeChange = new UnityEvent<object>();

    private static TraceMode _selectedTraceMode;

    public static TraceMode SelectedTraceMode
    {
        get => _selectedTraceMode;
    }

    public static void LoadTraceMode()
    {
        TraceMode selectedTraceMode = (TraceMode)PlayerPrefs.GetInt(nameof(TraceMode), (int)TraceMode.Depth);
        _selectedTraceMode = selectedTraceMode;
        OnTraceModeChange.Invoke(selectedTraceMode);
    }

    public static void SetTraceMode(object newTraceMode)
    {
        TraceMode traceMode = (TraceMode)newTraceMode;
        _selectedTraceMode = traceMode;
        PlayerPrefs.SetInt(nameof(TraceMode), (int)traceMode);
        OnTraceModeChange.Invoke(traceMode);
    }

    //############################//
    //                            //
    //         Clustering         //
    //                            //
    //############################//

    public enum BehaviorGraphClustering
    {
        Enabled,
        Disabled,
    }

    public static UnityEvent<object> OnClusteringChange = new UnityEvent<object>();

    private static BehaviorGraphClustering _selectedClustering;

    public static BehaviorGraphClustering SelectedClustering
    {
        get => _selectedClustering;
    }

    public static void LoadClustering()
    {
        BehaviorGraphClustering selectedClustering = (BehaviorGraphClustering)PlayerPrefs.GetInt(nameof(BehaviorGraphClustering), (int)BehaviorGraphClustering.Enabled);
        _selectedClustering = selectedClustering;
        OnClusteringChange.Invoke(selectedClustering);
    }

    public static void SetClustering(object newClustering)
    {
        BehaviorGraphClustering clustering = (BehaviorGraphClustering)newClustering;
        _selectedClustering = clustering;
        PlayerPrefs.SetInt(nameof(BehaviorGraphClustering), (int)clustering);
        OnClusteringChange.Invoke(clustering);
    }

    ////////////////////////////////

    private static List<Tuple<Type, Action<object>, Action, UnityEvent<object>>> _selectableSettings = new List<Tuple<Type, Action<object>, Action, UnityEvent<object>>>(
        new Tuple<Type, Action<object>, Action, UnityEvent<object>>[]
        {
            new Tuple<Type, Action<object>, Action, UnityEvent<object>>(typeof(ClassDiagramRelationType), SetRelationType, LoadRelationType, OnRelationTypeChange),
            new Tuple<Type, Action<object>, Action, UnityEvent<object>>(typeof(BehaviorMode), SetBehaviorMode, LoadBehaviorMode, OnBehaviorModeChange),
            new Tuple<Type, Action<object>, Action, UnityEvent<object>>(typeof(TraceMode), SetTraceMode, LoadTraceMode, OnTraceModeChange),
            new Tuple<Type, Action<object>, Action, UnityEvent<object>>(typeof(BehaviorGraphClustering), SetClustering, LoadClustering, OnClusteringChange),
        }
    );

    //############################//
    //                            //
    //            Misc            //
    //                            //
    //############################//

    public enum Misc
    {
        ResetSelection,
        ResetSettings,
    }

    private static void OnMiscInteracted(object selectedMiscObject)
    {
        Misc selectedMisc = (Misc)selectedMiscObject;

        switch (selectedMisc)
        {
            case Misc.ResetSelection:
                BehaviorGraph.ClearSelection();
                break;
            case Misc.ResetSettings:
                PlayerPrefs.DeleteAll();
                foreach (Tuple<Type, Action<object>, Action, UnityEvent<object>> setting in _selectableSettings)
                {
                    // Load default setting
                    setting.Item3();
                }
                break;
        }
    }

    void Awake()
    {
        foreach (Tuple<Type, Action<object>, Action, UnityEvent<object>> setting in _selectableSettings)
        {
            GenerateSelectableSetting(setting.Item1, setting.Item2, setting.Item4);

            setting.Item3(); // Load setting from PlayerPrefs (Invokes ChangeEvent)
        }

        GenerateSetting(typeof(Misc), OnMiscInteracted);
    }

    private void GenerateSetting(Type enumType, Action<object> onInteract)
    {
        GameObject settingObject = Instantiate(settingPrefab, background);
        Setting setting = settingObject.GetComponent<Setting>();
        setting.Initialize(enumType, onInteract);
    }

    private void GenerateSelectableSetting(Type enumType, Action<object> onInteract, UnityEvent<object> changeEvent)
    {
        GameObject settingObject = Instantiate(selectableSettingPrefab, background);
        SelectableSetting setting = settingObject.GetComponent<SelectableSetting>();
        setting.Initialize(enumType, onInteract, changeEvent);
    }
}
