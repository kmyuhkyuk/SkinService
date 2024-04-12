using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SkinService.Views
{
    public class SkinServiceConfig : SkinServiceBase
    {
        [SerializeField] private TMP_Dropdown dropdown;

        [SerializeField] private Button resetButton;

        [SerializeField] private TMP_Text resetName;

        private const string ResetNameKey = "Reset";

        public int CurrentIndex
        {
            get => dropdown.value;
            set
            {
                if (dropdown.value == value)
                    return;

                dropdown.value = value;
            }
        }

        public void Init(string nameKey, Action onResetClick)
        {
            Init(nameKey);
            resetName.text = ResetNameKey;
            resetButton.onClick.AddListener(new UnityAction(onResetClick));
        }

        public void UpdateConfig(List<string> options, int index)
        {
            UpdateDropdown(options);
            CurrentIndex = index;
            UpdateConfigName();
        }

        public void UpdateDropdown(List<string> options)
        {
            ClearDropdown();
            dropdown.AddOptions(options);
        }

        public void ClearDropdown()
        {
            dropdown.ClearOptions();
        }
    }
}