using System.Collections.Generic;
using Ricimi;

public class UnitsButton : PopupOpener
{
    private string context = "UnitButton";
    public void OnClick()
    {
        Logger.LogInfo($"Clicked button name is {transform.parent.name.ToLower()} from unit{transform.parent.parent.name.Substring(transform.parent.parent.name.Length - 2)}", context);
        Dictionary<string, bool> _data = transform.parent.parent.GetComponent<CheckUnitStageButtonStatus>().unitButtonStatusForClick;
        var key = transform.parent.name.ToLower();
        if (!_data.TryGetValue(key, out bool isLocked))
        {
            OpenPopup();
            return;
        }

        if (isLocked == true)
        {
            OpenPopup();
        }
        else
        {
            PlayerInfo.UnitButtonInfo.Clear();
            PlayerInfo.UnitButtonInfo.Add(transform.parent.parent.name.Substring(transform.parent.parent.name.Length - 2), transform.parent.name.ToLower());
            Logger.LogInfo($"Trivia quiz name set to {transform.parent.name.ToLower()}", context);
            TransitionAdditive.LoadLevel(transform.parent.name, Params.SceneTransitionDuration, Params.SceneTransitionColor);

        }

    }
}
