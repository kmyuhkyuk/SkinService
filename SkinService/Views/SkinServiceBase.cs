using TMPro;
using UnityEngine;

namespace SkinService.Views
{
    public abstract class SkinServiceBase : MonoBehaviour
    {
        [SerializeField] protected TMP_Text configName;

        protected string ConfigNameKey;

        public void Init(string nameKey)
        {
            ConfigNameKey = nameKey;

            UpdateConfigName();
        }

        public void UpdateConfigName()
        {
            configName.text = ConfigNameKey;
        }
    }
}