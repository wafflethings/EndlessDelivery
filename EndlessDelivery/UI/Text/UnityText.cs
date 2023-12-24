using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    public class UnityText : MonoBehaviour, IText
    {
        private Text _text;

        private void Start()
        {
            _text = GetComponent<Text>();
        }
        
        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}