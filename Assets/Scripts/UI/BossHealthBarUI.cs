using UnityEngine;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>Shows the health of the active boss; hidden when no boss is present.</summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Tooltip("Image of type Filled (horizontal) whose fillAmount mirrors boss health.")]
        [SerializeField] Image fill;

        Boss boss;

        /// <summary>Binds the bar to <paramref name="boss"/> and makes it visible.</summary>
        public void Show(Boss target)
        {
            Unbind();
            boss = target;
            boss.HealthChanged += SetFill;
            SetFill(1f);
            gameObject.SetActive(true);
        }

        /// <summary>Unbinds from the boss and hides the bar.</summary>
        public void Hide()
        {
            Unbind();
            gameObject.SetActive(false);
        }

        void OnDestroy() => Unbind();

        void SetFill(float normalized) => fill.fillAmount = normalized;

        void Unbind()
        {
            if (boss) boss.HealthChanged -= SetFill;
            boss = null;
        }
    }
}
