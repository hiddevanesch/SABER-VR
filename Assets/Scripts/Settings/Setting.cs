using System;
using TMPro;
using UnityEngine;
using Util;

public class Setting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject optionPrefab;
    [SerializeField] private TextMeshProUGUI settingText;
    [SerializeField] protected RectTransform optionsObject;

    protected Type _enumType;
    protected Action<object> _changeOption;

    public void Initialize(Type enumType, Action<object> changeOption)
    {
        _enumType = enumType;
        _changeOption = changeOption;

        UpdateText();
        GenerateOptions();
    }

    protected virtual void GenerateOptions()
    {
        foreach (var enumItem in Enum.GetValues(_enumType))
        {
            GameObject optionObject = Instantiate(optionPrefab, optionsObject);
            Option option = optionObject.GetComponent<Option>();
            option.Initialize(_enumType, enumItem, _changeOption);
        }
    }

    private void UpdateText()
    {
        settingText.text = StringUtil.SplitCamelCase(_enumType.Name);
    }
}
