using UnityEngine;
using UnityEngine.UIElements;

namespace Base_Classes
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class ViewBase : MonoBehaviour
    {
        protected UIDocument Document { get; private set; }
        protected VisualElement Root { get; private set; }

        public bool IsVisible { get; private set; }
        protected bool IsInitialized { get; private set; }

        protected virtual void Awake()
        {
            Document = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            EnsureInitialized();
        }

        protected bool EnsureInitialized()
        {
            if (IsInitialized) return true;

            if (Document == null)
            {
                Debug.LogError($"[{GetType().Name}] UIDocument component is missing.");
                return false;
            }

            Root = Document.rootVisualElement;
            if (Root == null)
            {
                Debug.LogError($"[{GetType().Name}] Root visual element is not available.");
                return false;
            }

            OnInitializeUI();
            IsInitialized = true;
            return true;
        }

        protected abstract void OnInitializeUI();

        public virtual void Show()
        {
            if (!EnsureInitialized()) return;

            Root.style.display = DisplayStyle.Flex;
            IsVisible = true;
        }

        public virtual void Hide()
        {
            if (!EnsureInitialized()) return;

            Root.style.display = DisplayStyle.None;
            IsVisible = false;
        }
    }
}