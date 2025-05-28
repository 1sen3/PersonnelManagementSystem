using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FluentIcons.Common;
using FluentIcons.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using static System.Collections.Specialized.BitVector32;
using PersonnelManagementSystem.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PersonnelManagementSystem.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        private Dictionary<string, (FluentIcon icon, TextBlock text)> _navItems;
        private Dictionary<string, (Storyboard moveUp, Storyboard moveDown)> _navItemAnimations;
        private string _previousSelectedTag = null;
        public AdminPage()
        {
            InitializeComponent();

            InitializeNavItems();
            InitializeNavItemAnimations();

            nav.SelectedItem = Home;
            frame.Navigate(typeof(StaffListPage));
        }

        private void nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            string selected = args.SelectedItemContainer.Tag.ToString();
            UpdateNavItemsAppearance(selected);
            switch (selected)
            {
                case "Home":
                    frame.Navigate(typeof(HomePage));
                    break;
                case "StaffList":
                    frame.Navigate(typeof(StaffListPage));
                    break;
                case "ChangeStaff":
                    frame.Navigate(typeof(ChangeStaffPage));
                    break;
                case "Settings":
                    frame.Navigate(typeof(SettingsPage));
                    break;
            }
        }

        private void InitializeNavItems()
        {
            _navItems = new Dictionary<string, (FluentIcon icon, TextBlock text)>
            {
                { "Home", (HomeIcon, HomeText) },
                { "StaffList", (StaffListIcon, StaffListText) },
                { "ChangeStaff", (ChangeStaffIcon, ChangeStaffText) },
                { "Settings", (SettingsIcon, SettingsText) }
            };
        }
        private void InitializeNavItemAnimations()
        {
            _navItemAnimations = new Dictionary<string, (Storyboard, Storyboard)>();

            SetupAnimationForNavItem("Home", HomeIcon, HomeText);
            SetupAnimationForNavItem("StaffList", StaffListIcon, StaffListText);
            SetupAnimationForNavItem("ChangeStaff", ChangeStaffIcon, ChangeStaffText);
            SetupAnimationForNavItem("Settings", SettingsIcon, SettingsText);
        }
        private void SetupAnimationForNavItem(string tag, FluentIcon icon, TextBlock text)
        {
            Storyboard moveDownAnimation = this.Resources["IconMoveDownAnimation"] as Storyboard;
            Storyboard moveDownClone = new Storyboard();

            foreach (Timeline timeline in moveDownAnimation.Children)
            {
                if (timeline is DoubleAnimation doubleAnim)
                {
                    DoubleAnimation newAnim = new DoubleAnimation
                    {
                        From = doubleAnim.From,
                        To = doubleAnim.To,
                        Duration = doubleAnim.Duration,
                        EasingFunction = doubleAnim.EasingFunction
                    };

                    if (Storyboard.GetTargetName(doubleAnim) == "TextBlockTarget")
                    {
                        Storyboard.SetTarget(newAnim, text);
                        Storyboard.SetTargetProperty(newAnim, Storyboard.GetTargetProperty(doubleAnim));
                    }
                    else
                    {
                        Storyboard.SetTarget(newAnim, icon);
                        Storyboard.SetTargetProperty(newAnim, Storyboard.GetTargetProperty(doubleAnim));
                    }

                    moveDownClone.Children.Add(newAnim);
                }
            }

            Storyboard moveUpAnimation = this.Resources["IconMoveUpAnimation"] as Storyboard;
            Storyboard moveUpClone = new Storyboard();

            foreach (Timeline timeline in moveUpAnimation.Children)
            {
                if (timeline is DoubleAnimation doubleAnim)
                {
                    DoubleAnimation newAnim = new DoubleAnimation
                    {
                        From = doubleAnim.From,
                        To = doubleAnim.To,
                        Duration = doubleAnim.Duration,
                        EasingFunction = doubleAnim.EasingFunction
                    };

                    if (Storyboard.GetTargetName(doubleAnim) == "TextBlockTarget")
                    {
                        Storyboard.SetTarget(newAnim, text);
                        Storyboard.SetTargetProperty(newAnim, Storyboard.GetTargetProperty(doubleAnim));
                    }
                    else
                    {
                        Storyboard.SetTarget(newAnim, icon);
                        Storyboard.SetTargetProperty(newAnim, Storyboard.GetTargetProperty(doubleAnim));
                    }

                    moveUpClone.Children.Add(newAnim);
                }
            }

            _navItemAnimations[tag] = (moveDownClone, moveUpClone);
        }
        private void UpdateNavItemsAppearance(string selectedTag)
        {
            var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];

            foreach (var item in _navItems)
            {
                bool isSelected = item.Key == selectedTag;
                bool wasPreviouslySelected = item.Key == _previousSelectedTag;

                item.Value.icon.IconVariant = isSelected ? IconVariant.Filled : IconVariant.Regular;

                item.Value.icon.Foreground = isSelected ? accentBrush :
                    (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"];

                if (isSelected)
                {
                    _navItemAnimations[item.Key].moveUp.Begin();
                }
                else if (wasPreviouslySelected)
                {
                    _navItemAnimations[item.Key].moveDown.Begin();
                }
            }

            _previousSelectedTag = selectedTag;
        }
    }
}
