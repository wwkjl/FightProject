using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;

public class UIPopupSystem : MonoBehaviour
{
    public string text = "";
    public Text PopupText;

    UIPopup instancePopup;

    private void OnEnable()
    {
        PopupText.text = text;
    }

    public void PopupShow(string text)
    {
        this.text = text;
        UIPopup uIPopup = UIPopupManager.GetPopup("System");
        uIPopup.SetTargetCanvasName("MasterCanvas");
        uIPopup.Show();
        instancePopup = uIPopup;
    }

    public void PopupHide()
    {
        if (instancePopup != null) instancePopup.Hide();
    }
}
