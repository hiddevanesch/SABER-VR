using System;
using TMPro;
using UnityEngine;
using Util;

public class Option : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] protected TextMeshProUGUI optionText;

    protected object _enumItem;
    private Type _enumType;
    private Action<object> _changeOption;

    public void Initialize(Type enumType, object enumItem, Action<object> changeOption)
    {
        _enumType = enumType;
        _enumItem = enumItem;
        _changeOption = changeOption;

        UpdateText();
    }

    private void UpdateText()
    {
        optionText.text = StringUtil.SplitCamelCase(Enum.GetName(_enumType, _enumItem));
    }

    //############################//
    //                            //
    //       IInteractable        //
    //                            //
    //############################//

    public void Interact()
    {
        _changeOption(_enumItem);
    }
}
