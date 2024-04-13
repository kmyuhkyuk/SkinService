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
            resetButton.onClick.AddListener(new UnityAction(onResetClick));
        }

        public void UpdateConfig(List<string> options, int index)
        {
            UpdateDropdown(options);
            CurrentIndex = index;
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

        protected override void UpdateLocalized()
        {
#if !UNITY_EDITOR
            base.UpdateLocalized();
            resetName.text = Helpers.LocalizedHelper.Instance.Localized(ResetNameKey);

#endif
        }
    }
}