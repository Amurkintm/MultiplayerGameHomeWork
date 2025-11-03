using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private RectTransform _filledImage;
    [SerializeField] private float _defoultWidth;
    private void OnValidate()
    {
        _defoultWidth = _filledImage.sizeDelta.x;
    }

    // Update is called once per frame
    public void UpdateHealth(float max, int current)
    {
        float percent = current / max;
        _filledImage.sizeDelta = new Vector2(_defoultWidth * percent, _filledImage.sizeDelta.y);
    }
}
