using System;
using TMPro;
using UnityEngine.Events;

public class SelectableOption : Option
{
    public void Initialize(Type enumType, object enumItem, Action<object> changeOption, UnityEvent<object> changeEvent)
    {
        Initialize(enumType, enumItem, changeOption);

        changeEvent.AddListener(UpdateLooks);
    }

    private void UpdateLooks(object selectedEnumItem)
    {
        if (selectedEnumItem.Equals(_enumItem))
        {
            optionText.fontStyle = FontStyles.Underline;
        }
        else
        {
            optionText.fontStyle = FontStyles.Normal;
        }
    }
}
