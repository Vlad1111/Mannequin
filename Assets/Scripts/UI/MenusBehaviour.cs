using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenusBehaviour : MonoBehaviour
{
    public static MenusBehaviour Instance;
    private void Awake()
    {
        Instance = this;
    }
    public Transform menu;
    public Transform[] menuTabs;
    public Transform bodyMenuTab;
    public Transform poseMenuTab;
    private int menuTabIndex = 0;
    public TextMeshProUGUI tabName;
    public SettingsMenuBehaviour settingsMenuBehaviour;

    private void Start()
    {
        NextMenuTab(0);
        settingsMenuBehaviour.Start();
    }

    public void ToggleIngormation(Transform informationParent)
    {
        informationParent.gameObject.SetActive(!informationParent.gameObject.activeSelf);
    }

    public void ToggleMenus()
    {
        menu.gameObject.SetActive(!menu.gameObject.activeSelf);
    }

    private void ToggleOffAllTabs()
    {
        foreach (var tab in menuTabs)
            tab.gameObject.SetActive(false);

        bodyMenuTab.gameObject.SetActive(false);
        poseMenuTab.gameObject.SetActive(false);
    }

    private void ToggleTabOn(Transform tab)
    {
        tab.gameObject.SetActive(true);
        tabName.text = tab.name;
    }

    public void NextMenuTab(int direction)
    {
        menuTabIndex += direction;
        if (menuTabIndex >= menuTabs.Length)
            menuTabIndex = 0;
        else if (menuTabIndex < 0)
            menuTabIndex = menuTabs.Length - 1;

        if(WorldMenuBehaviour.Instance?.lastObjectSelected?.creator != null)
        {
            var creator = WorldMenuBehaviour.Instance.lastObjectSelected?.creator;
            if((menuTabs[menuTabIndex] == bodyMenuTab && creator.sliderDatas.Length <= 0) ||
                (menuTabs[menuTabIndex] == poseMenuTab && creator.poseSliderDatas.Length <= 0))
            {
                NextMenuTab(direction);
                return;
            }
        }

        ToggleOffAllTabs();
        ToggleTabOn(menuTabs[menuTabIndex]);
    }

    public void SwitchToBodyTab()
    {
        menuTabIndex = 1;
        ToggleOffAllTabs();
        ToggleTabOn(bodyMenuTab);
    }

    public void SwitchToPoseTab()
    {
        menuTabIndex = 2;
        ToggleOffAllTabs();
        ToggleTabOn(poseMenuTab);
    }
}
