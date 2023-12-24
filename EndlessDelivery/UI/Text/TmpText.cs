using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    public class TmpText : MonoBehaviour, IText
    {
        private TMP_Text _text;

        private void Start()
        {
            _text = GetComponent<TMP_Text>();
        }
        
        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}