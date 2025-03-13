using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuEntry : MonoBehaviour
{
    public Button rebindButton;
    public TextMeshProUGUI rebindButtonLabel;
    public Button clearButton;
    public Button resetButton;
    public TextMeshProUGUI inputActionLabel;

    public static void ConnectNavigation(ControlsMenuEntry a, ControlsMenuEntry b)
    {
        Navigation nav;
        nav = a.rebindButton.navigation;
        nav.selectOnDown = b.rebindButton;
        a.rebindButton.navigation = nav;
        nav = b.rebindButton.navigation;
        nav.selectOnUp = a.rebindButton;
        b.rebindButton.navigation = nav;
        nav = a.clearButton.navigation;
        nav.selectOnDown = b.clearButton;
        a.clearButton.navigation = nav;
        nav = b.clearButton.navigation;
        nav.selectOnUp = a.clearButton;
        b.clearButton.navigation = nav;
        nav = a.resetButton.navigation;
        nav.selectOnDown = b.resetButton;
        a.resetButton.navigation = nav;
        nav = b.resetButton.navigation;
        nav.selectOnUp = a.resetButton;
        b.resetButton.navigation = nav;
    }

    public static void ConnectNavigation(Selectable a, ControlsMenuEntry b)
    {
        Navigation nav;
        nav = a.navigation;
        nav.selectOnDown = b.rebindButton;
        a.navigation = nav;
        nav = b.rebindButton.navigation;
        nav.selectOnUp = a;
        b.rebindButton.navigation = nav;
        nav = b.clearButton.navigation;
        nav.selectOnUp = a;
        b.clearButton.navigation = nav;
        nav = b.resetButton.navigation;
        nav.selectOnUp = a;
        b.resetButton.navigation = nav;
    }

    public static void ConnectNavigation(ControlsMenuEntry a, Selectable b)
    {
        Navigation nav;
        nav = a.rebindButton.navigation;
        nav.selectOnDown = b;
        a.rebindButton.navigation = nav;
        nav = a.clearButton.navigation;
        nav.selectOnDown = b;
        a.clearButton.navigation = nav;
        nav = a.resetButton.navigation;
        nav.selectOnDown = b;
        a.resetButton.navigation = nav;
        nav = b.navigation;
        nav.selectOnUp = a.rebindButton;
        b.navigation = nav;
    }
}
