using Nexora.Experimental.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.UI
{
    /// <summary>
    /// Displays messages received from the sender onto the screen.
    /// Applies fade and delay, and has limit for maximum message instances available.
    /// </summary>
    /// <remarks>
    /// Derive from this class and specific <see cref="IMessageListener{TSender}"/> to implement your own custom logic
    /// for different sender types.
    /// </remarks>
    /// <typeparam name="T">Type of the sender.</typeparam>
    public abstract class MessageDisplayer : MonoBehaviour 
    {
        [SerializeField]
        private GameObject _messageTemplatePrefab;

        [SerializeField, Range(1, 30)]
        private int _maxMessageInstances;

        [SerializeField, Range(0f, 10f), Title("Fading")]
        private float _fadeDelay = 2f;

        [SerializeField, Range(0, 10f)]
        private float _fadeDuration = 1f;

        [SerializeField, Title("Colors")]
        private Color _infoMessageColor;

        [SerializeField]
        private Color _warningMessageColor;

        [SerializeField]
        private Color _errorMessageColor;

        private MessageInstance[] _messageInstances;
        private int _currentInstanceIndex = -1;

        private MessageInstance NextAvailableInstance() =>
            _messageInstances.SelectSequence(ref _currentInstanceIndex);

        public void DisplayMessage(in MessageArgs args)
        {
            var color = GetMessageColor(args.MessageType);
            var messageInstance = NextAvailableInstance();
            messageInstance.Show(args.Message, color.WithAlpha(1), args.Sprite, _fadeDelay, _fadeDuration);
        }

        private void Awake()
        {
            InitializeMessageInstances();
        }

        private void InitializeMessageInstances()
        {
            _messageInstances = new MessageInstance[_maxMessageInstances];
            for(int i = 0; i < _messageInstances.Length; i++) 
            {
                _messageInstances[i] = new MessageInstance(_messageTemplatePrefab, transform);
            }
        }

        private Color GetMessageColor(MessageType type) => type switch
        {
            MessageType.Info => _infoMessageColor,
            MessageType.Warning => _warningMessageColor,
            MessageType.Error => _errorMessageColor,
            _ => Color.black
        };

        private sealed class MessageInstance
        {
            public readonly GameObject Root;
            public readonly TextMeshProUGUI Text;
            public readonly Image Icon;
            public readonly CanvasGroup CanvasGroup;

            public MessageInstance(GameObject prefab, Transform parent)
            {
                Root = Instantiate(prefab, parent);
                Text = Root.GetComponentInChildren<TextMeshProUGUI>();
                Icon = Root.transform.Find("Icon").GetComponent<Image>();
                CanvasGroup = Root.GetComponentInChildren<CanvasGroup>();
                CanvasGroup.alpha = 0;
            }

            public void Show(
                string text, 
                Color textColor, 
                Sprite icon, 
                float delay, 
                float duration)
            {
                Root.SetActive(true);
                Root.transform.SetAsLastSibling();

                Text.text = text;
                Text.color = textColor;

                Icon.gameObject.SetActive(icon != null);
                Icon.sprite = icon;

                CanvasGroup.alpha = 1;
                CanvasGroup.TweenAlpha(0f, duration).SetDelay(delay).SetAutoReleaseWithParent(true);
            }

            private void ResetState()
            {
                CanvasGroup.alpha = 0;
                Root.SetActive(false);
            }
        }
    }

    /* Example message displayer definition
    public class ObjectMessageDisplayer :
        MessageDisplayer,
        IMessageListener<UnityEngine.Object>
    {
        public void OnMessageReceived(UnityEngine.Object sender, in MessageArgs args)
        {
            DisplayMessage(in args);
        }

        private void OnEnable() => MessageDispatcher<UnityEngine.Object>.Instance.AddListener(this);
        private void OnDisable() => MessageDispatcher<UnityEngine.Object>.Instance.RemoveListener(this);
    }
    */
}
