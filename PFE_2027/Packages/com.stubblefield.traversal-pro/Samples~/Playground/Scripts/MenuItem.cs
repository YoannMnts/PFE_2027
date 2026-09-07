using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraversalPro.Samples.Playground
{
    public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] GraphicAnimation text;
        [SerializeField] GraphicAnimation background;
        [SerializeField] Button button;

        public bool IsHovered
        {
            get => text.isHovered;
            set
            {
                if (value == IsHovered) return;
                text.isHovered = value;
                background.isHovered = value;
                if (value) Hovered?.Invoke(this);
            }
        }

        public event Action<MenuItem> Clicked;
        public event Action<MenuItem> Hovered;

        void Awake()
        {
            text.Init();
            background.Init();
        }

        void Start()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(this));
        }

        void Update()
        {
            text.Update();
            background.Update();
        }

        public void CompleteAnimation()
        {
            text.CompleteAnimation();
            background.CompleteAnimation();
        }

        public void Click() => button.onClick.Invoke();
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
        }

        [Serializable]
        class GraphicAnimation
        {
            public Graphic graphic;
            public Color hoverColor = Color.yellow;
            public Vector3 hoverScale = Vector3.one * 1.1f;
            public bool isHovered;
            public float smoothTime = .05f;
            Color baseColor;
            Vector3 baseScale;
            Vector4 colorVelocity;
            Vector3 scaleVelocity;

            public void Init()
            {
                baseColor = graphic.color;
                baseScale = graphic.transform.localScale;
            }

            public void Update()
            {
                Vector4 currentColor = ColorToVector(graphic.color);
                Vector4 goalColor = ColorToVector(isHovered ? hoverColor : baseColor);
                currentColor = SmoothDamp(currentColor, goalColor, ref colorVelocity, smoothTime);
                graphic.color = new Color(currentColor.x, currentColor.y, currentColor.z, currentColor.w);
                Vector3 scaleGoal = isHovered ? hoverScale : baseScale;
                graphic.transform.localScale = Vector3.SmoothDamp(graphic.transform.localScale, scaleGoal, ref scaleVelocity, smoothTime, float.MaxValue, Time.unscaledDeltaTime);
            }

            public void CompleteAnimation()
            {
                graphic.color = isHovered ? hoverColor : baseColor;
                graphic.transform.localScale = isHovered ? hoverScale : baseScale;
            }
            
            static Vector4 ColorToVector(Color value) => new Vector4(value.r, value.g, value.b, value.a);

            static Vector4 SmoothDamp(Vector4 current, Vector4 target, ref Vector4 velocity, float smoothTime)
            {
                Vector3 vel = xyz(velocity);
                Vector3 cur = Vector3.SmoothDamp(xyz(current), xyz(target), ref vel, smoothTime, float.MaxValue, Time.unscaledDeltaTime);
                current.x = cur.x;
                current.y = cur.y;
                current.z = cur.z;
                velocity.x = vel.x;
                velocity.y = vel.y;
                velocity.z = vel.z;
                current.w = Mathf.SmoothDamp(current.w, target.w, ref velocity.w, smoothTime, float.MaxValue, Time.unscaledDeltaTime);
                return current;
            }

            static Vector3 xyz(Vector4 value) => new Vector3(value.x, value.y, value.z);
        }
    }
}