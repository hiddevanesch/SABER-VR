using System;
using UnityEngine;
using UnityEngine.Events;

public class SelectableSetting : Setting
{
    private UnityEvent<object> _changeEvent;

    public void Initialize(Type enumType, Action<object> changeOption, UnityEvent<object> changeEvent)
    {
        _changeEvent = changeEvent;

        Initialize(enumType, changeOption);
    }

    protected override void GenerateOptions()
    {
        foreach (var enumItem in Enum.GetValues(_enumType))
        {
            GameObject optionObject = Instantiate(optionPrefab, optionsObject);
            SelectableOption option = optionObject.GetComponent<SelectableOption>();
            option.Initialize(_enumType, enumItem, _changeOption, _changeEvent);
        }
    }
}
