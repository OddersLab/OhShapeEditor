/// https://answers.unity.com/questions/1294584/how-to-disable-selectall-text-of-inputfield-onfocu.html
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomInputField : InputField
{
    bool focused = false;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        focused = true;
    }
    
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        focused = false;
    }

    protected override void LateUpdate()
    {       
        base.LateUpdate();
        if (focused)
        {
            focused = false;
            MoveTextEnd(true);
        }
    }
}