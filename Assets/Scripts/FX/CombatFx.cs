using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if DOTWEEN || DOTWEEN_PRESENT
using DG.Tweening;
#endif

namespace AutoBattlerSpire.FX
{
    public static class CombatFx
    {
        public static void TweenHp(Slider slider, float to, float dur = 0.25f)
        {
            if (slider == null) return;

#if DOTWEEN || DOTWEEN_PRESENT
            if (DOTween.IsTweening(slider, true))
            {
                slider.DOKill(true);
            }
            slider.DOValue(to, dur).SetEase(Ease.OutQuad);
#else
            slider.value = to;
#endif
        }

        public static void TweenBlock(TextMeshProUGUI label, int value)
        {
            if (label == null) return;

#if DOTWEEN || DOTWEEN_PRESENT
            label.transform.DOKill(true);
            label.text = $"Block: {value}";
            label.transform.localScale = Vector3.one * 1.15f;
            label.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
#else
            label.text = $"Block: {value}";
#endif
        }
    }
}

