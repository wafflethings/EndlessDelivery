using UnityEngine;

namespace EndlessDelivery.UI;

public class UnityText : MonoBehaviour, IText
{
    private UnityEngine.UI.Text _text;

    public void SetText(string text)
    {
        if (_text == null)
        {
            _text = GetComponent<UnityEngine.UI.Text>();
        }

        _text.text = text;
    }
}
