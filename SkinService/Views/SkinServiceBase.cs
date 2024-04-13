using TMPro;
using UnityEngine;

namespace SkinService.Views
{
    public abstract class SkinServiceBase : MonoBehaviour
    {
        [SerializeField] protected TMP_Text configName;

        protected string ConfigNameKey;

        protected void Start()
        {
#if !UNITY_EDITOR

            Helpers.LocalizedHelper.Instance.LanguageChange += UpdateLocalized;

#endif
        }

        public void Init(string nameKey)
        {
            ConfigNameKey = nameKey;

            UpdateLocalized();
        }

        protected virtual void UpdateLocalized()
        {
#if !UNITY_EDITOR
            configName.text = Helpers.LocalizedHelper.Instance.Localized(ConfigNameKey);

#endif
        }
    }
}