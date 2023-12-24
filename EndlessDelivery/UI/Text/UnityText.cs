using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    public class UnityText : MonoBehaviour, IText
    {
        private Text _text;

        public void SetText(string text)
        {
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }
            
            _text.text = text;
        }
    }
}