// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Synty.SidekickCharacters.Synty.SidekickCharacters.Scripts.Editor.UI
{
    /// <summary>
    ///     Makes a PopupField/DropdownField open a scrollable UIToolkit dropdown menu instead of the native OS context
    ///     menu, which handles long choice lists poorly (no proper scrollbar).
    /// </summary>
    public static class ScrollableDropdown
    {
        /// <summary>
        ///     Replaces the given field's built-in dropdown menu with a scrollable GenericDropdownMenu.
        ///     Selection still goes through field.value, so existing value-changed callbacks are unaffected.
        /// </summary>
        /// <param name="field">The dropdown field to modify.</param>
        public static void Apply(PopupField<string> field)
        {
            Register(field, () => field.choices, () => field.value, newValue => field.value = newValue);
        }

        /// <summary>
        ///     Replaces the given field's built-in dropdown menu with a scrollable GenericDropdownMenu.
        ///     Selection still goes through field.value, so existing value-changed callbacks are unaffected.
        /// </summary>
        /// <param name="field">The dropdown field to modify.</param>
        public static void Apply(DropdownField field)
        {
            Register(field, () => field.choices, () => field.value, newValue => field.value = newValue);
        }

        /// <summary>
        ///     Registers the event interceptors that suppress the field's built-in menu and show the scrollable one.
        /// </summary>
        /// <param name="field">The dropdown field element.</param>
        /// <param name="getChoices">Getter for the field's current choices.</param>
        /// <param name="getValue">Getter for the field's current value.</param>
        /// <param name="setValue">Setter applying a newly selected value to the field.</param>
        private static void Register(VisualElement field, Func<List<string>> getChoices, Func<string> getValue, Action<string> setValue)
        {
            field.RegisterCallback<PointerDownEvent>(
                evt =>
                {
                    SuppressBuiltInMenu(evt);
                    ShowMenu(field, getChoices, getValue, setValue);
                },
                TrickleDown.TrickleDown
            );

            // Block the compatibility mouse event as well so the built-in menu can never open; the menu itself is
            // only shown from the pointer event above to avoid opening twice.
            field.RegisterCallback<MouseDownEvent>(
                evt =>
                {
                    SuppressBuiltInMenu(evt);
                },
                TrickleDown.TrickleDown
            );

            field.RegisterCallback<KeyDownEvent>(
                evt =>
                {
                    if (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        SuppressBuiltInMenu(evt);
                        ShowMenu(field, getChoices, getValue, setValue);
                    }
                },
                TrickleDown.TrickleDown
            );
        }

        /// <summary>
        ///     Stops the event from reaching the field's built-in menu handling.
        /// </summary>
        /// <param name="evt">The event to suppress.</param>
        private static void SuppressBuiltInMenu(EventBase evt)
        {
            evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
            // In older Unity versions the built-in menu opens from the event's default action, which is only
            // blocked by PreventDefault. In newer versions PreventDefault is obsolete and stopping propagation
            // is sufficient.
            evt.PreventDefault();
#endif
        }

        /// <summary>
        ///     Shows the scrollable dropdown menu anchored to the given field, built from its current choices.
        /// </summary>
        /// <param name="field">The dropdown field element.</param>
        /// <param name="getChoices">Getter for the field's current choices.</param>
        /// <param name="getValue">Getter for the field's current value.</param>
        /// <param name="setValue">Setter applying a newly selected value to the field.</param>
        private static void ShowMenu(VisualElement field, Func<List<string>> getChoices, Func<string> getValue, Action<string> setValue)
        {
            List<string> choices = getChoices();
            if (choices == null || choices.Count == 0)
            {
                return;
            }

            GenericDropdownMenu menu = new GenericDropdownMenu();
            string currentValue = getValue();
            // The value can lag behind what the field visibly shows (e.g. Parts tab dropdowns whose choices are
            // replaced after creation), so also match against the text displayed on the field itself.
            string currentText = field.Q<TextElement>(className: "unity-base-popup-field__text")?.text;

            foreach (string choice in choices)
            {
                string captured = choice;
                bool isChecked = Matches(choice, currentValue) || Matches(choice, currentText);
                menu.AddItem(choice, isChecked, () => setValue(captured));
            }

#if UNITY_6000_3_OR_NEWER
            // Fixed matches the old anchored=true behavior: the menu width matches the field's rect.
            menu.DropDown(field.worldBound, field, DropdownMenuSizeMode.Fixed);
#else
            menu.DropDown(field.worldBound, field, true);
#endif
        }

        /// <summary>
        ///     Compares a menu choice with the field's current value/text, tolerating whitespace and case differences.
        /// </summary>
        /// <param name="choice">The menu choice to test.</param>
        /// <param name="current">The field's current value or displayed text.</param>
        /// <returns>True if they refer to the same entry; otherwise false.</returns>
        private static bool Matches(string choice, string current)
        {
            return !string.IsNullOrEmpty(current)
                && string.Equals(choice?.Trim(), current.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
