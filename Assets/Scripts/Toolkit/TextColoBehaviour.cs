using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarWorld.Common.Utility
{
    [RequireComponent(typeof(Text))]
    public class TextColoBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Color _normalColor;

        [SerializeField]
        private Color _highlightColor;

        private Text _context;

        private void OnEnable()
        {
            if (!_context)
            {
                _context = GetComponent<Text>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _context.color = _highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _context.color = _normalColor;
        }
    }
}
