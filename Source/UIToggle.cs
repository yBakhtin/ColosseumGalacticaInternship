using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Colosseum.UI {
    public class UIToggle : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement {
        [SerializeField] private List<Graphic> onStateGraphics;
        [SerializeField] private List<Graphic> offStateGraphics;
        [SerializeField] private UIToggleGroup toggleGroup;
        [SerializeField] private bool isOn;
        [SerializeField] private bool allowSwitchOff = true;
        [Space, SerializeField] private ToggleEvent onStateChanged = new ToggleEvent();

        public bool IsOn {
            get {
                return isOn;
            }
            set {
                ChangeState(value);
            }
        }
        public bool InitialIsOn { get; private set; }
        public UIToggleGroup ToggleGroup { get { return toggleGroup; } }
        public ToggleEvent OnStateChanged { get { return onStateChanged; } }


        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // Only for input events for nu
            if (isOn && !allowSwitchOff)
                return;

            ChangeState(!isOn);
        }

        public virtual void OnSubmit(BaseEventData eventData) {
            ChangeState(!isOn); // Not sure if this is correct
        }

        public void LayoutComplete() {
            // Objects are destroyed at this moment
        }

        public void GraphicUpdateComplete() {
            ChangeGraphicalState(isOn);
        }

        public virtual void Rebuild(CanvasUpdate executing) {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onStateChanged.Invoke(isOn);
#endif
        }

        protected override void Start() {
            base.Start();
            ChangeGraphicalState(isOn);

            if (toggleGroup)
                toggleGroup.RegisterToggle(this);

            InitialIsOn = isOn;
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            ChangeGraphicalState(isOn);

            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
            if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

#endif // if UNITY_EDITOR

        protected override void OnEnable() {
            base.OnEnable();
            ChangeGraphicalState(isOn);
        }

        protected override void OnDisable() {
            base.OnDisable();
            ChangeGraphicalState(false);
        }

        private void ChangeState(bool value) {
            if (!IsActive() || !IsInteractable())
                return;

            bool stateChanged = false;

            ChangeGraphicalState(value);

            stateChanged = isOn != value;
            isOn = value;

            if (toggleGroup)
                toggleGroup.NotifyToggleStateChanged(this, isOn);

            if (stateChanged)
                onStateChanged.Invoke(isOn);
        }

        private void ChangeGraphicalState(bool enabled) {
            for (int i = 0; i < onStateGraphics.Count; i++)
                onStateGraphics[i].enabled = enabled;

            for (int i = 0; i < offStateGraphics.Count; i++)
                offStateGraphics[i].enabled = !enabled;
        }

        [System.Serializable] public class ToggleEvent : UnityEvent<bool> { }
    }

}