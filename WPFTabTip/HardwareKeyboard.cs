﻿using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace WPFTabTip
{
    public enum HardwareKeyboardIgnoreOptions
    {
        /// <summary>
        /// Do not ignore any keyboard.
        /// </summary>
        DoNotIgnore,

        /// <summary>
        /// Ignore keyboard, if there is only one, and it's description 
        /// can be found in ListOfHardwareKeyboardsToIgnoreIfSingleInstance.
        /// </summary>
        IgnoreIfSingleInstanceOnList,

        /// <summary>
        /// Ignore keyboard, if there is only one.
        /// </summary>
        IgnoreIfSingleInstance,

        /// <summary>
        /// Ignore all keyboards
        /// </summary>
        IgnoreAll
    }

    public enum PopupOptions
    {
        NoPopupOnTap,
        PopupOnTap = 1    
    }

    internal static class HardwareKeyboard
    {
        private static bool? _isConnected;

        /// <summary>
        /// Checks if Hardware Keyboard is Connected
        /// </summary>
        /// <returns></returns>
        internal static async Task<bool> IsConnectedAsync()
        {
            Task<bool> KeyboardConnectedCheckTask = Task.Run(() =>
            {
                SelectQuery SelectKeyboardsQuery = new SelectQuery("Win32_Keyboard");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(SelectKeyboardsQuery);
                ManagementObjectCollection Keyboards = Searcher.Get();

                if (Keyboards.Count == 0)
                    return false;

                if (Keyboards.Count > 1)
                    return true;

                if (Keyboards.Count == 1)
                {
                    switch (IgnoreOptions)
                    {
                        case HardwareKeyboardIgnoreOptions.IgnoreAll:
                            return false;
                        case HardwareKeyboardIgnoreOptions.IgnoreIfSingleInstance:
                            return false;
                        case HardwareKeyboardIgnoreOptions.IgnoreIfSingleInstanceOnList:
                            return !IsIgnoredKeyboard(Keyboards.Cast<ManagementBaseObject>().First());
                        default:
                            return true;
                    }
                }

                return false;
            });

#pragma warning disable 4014
            KeyboardConnectedCheckTask.ContinueWith(t => _isConnected = t.Result);
#pragma warning restore 4014

            if (_isConnected != null)
                return _isConnected.Value;
            else
                return await KeyboardConnectedCheckTask;
        }

        private static bool IsIgnoredKeyboard(ManagementBaseObject keyboard)
        {
            string description = keyboard.Properties.Cast<PropertyData>()
                .Where(k => k.Name == "Description")
                .Select(k => k.Value)
                .First()
                .ToString();

            return IgnoreIfSingleInstance.Contains(description);
        }

        internal static HardwareKeyboardIgnoreOptions IgnoreOptions = HardwareKeyboardIgnoreOptions.DoNotIgnore;

        /// <summary>
        /// Description of keyboards to ignore if there is only one instance of given keyboard.
        /// If you want to ignore some ghost keyboard, add it's description to this list
        /// </summary>
        internal static List<string> IgnoreIfSingleInstance { get; } = new List<string>();
    }
}
