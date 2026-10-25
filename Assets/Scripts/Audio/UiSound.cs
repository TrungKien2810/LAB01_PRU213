using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>
    /// Gives one menu button its "pointer moved onto me" tick. Menu screens attach it to every
    /// button at runtime (<see cref="Attach"/>), so hovering or arrowing between buttons is
    /// audible without any scene having to carry the component.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class UiSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        /// <summary>Plays the hover tick when the pointer moves onto the button.</summary>
        public void OnPointerEnter(PointerEventData eventData) => AudioDirector.Play(SoundEffect.UiHover);

        /// <summary>Plays the same tick when keyboard or gamepad selection moves onto the button.</summary>
        public void OnSelect(BaseEventData eventData) => AudioDirector.Play(SoundEffect.UiHover);

        /// <summary>
        /// Adds the tick to each of <paramref name="buttons"/>, skipping any that already have it
        /// or that were left empty in the Inspector.
        /// </summary>
        public static void Attach(params Button[] buttons)
        {
            foreach (Button button in buttons)
            {
                if (!button || button.TryGetComponent(out UiSound _)) continue;
                button.gameObject.AddComponent<UiSound>();
            }
        }
    }
}
