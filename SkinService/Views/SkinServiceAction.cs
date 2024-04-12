using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkinService.Views
{
    public class SkinServiceAction : SkinServiceBase, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image background;

        private Action configAction;

        private bool _isPointer;

        public void Init(string nameKey, Action action)
        {
            Init(nameKey);
            configAction = action;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPointer)
            {
                configAction();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointer = true;

            background.color = new Color(0.772549f, 0.7647059f, 0.6980392f, 0.7333333f);
            configName.color = Color.black;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointer = false;

            background.color = Color.black;
            configName.color = Color.white;
        }
    }
}