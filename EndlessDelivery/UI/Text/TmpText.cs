using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class TmpText : MonoBehaviour, IText
{
    private TMP_Text _text;

    public void SetText(string text)
    {
        if (_text == null)
        {
            _text = GetComponent<TMP_Text>();
        }

        _text.text = text;
    }
}
