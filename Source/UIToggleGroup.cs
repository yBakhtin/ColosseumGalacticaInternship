using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.UI {
    public class UIToggleGroup : MonoBehaviour {
        [SerializeField] private List<UIToggle> toggles = new List<UIToggle>();

        private List<UIToggle> activeToggles = new List<UIToggle>();
        private bool isBusy;


        //private void Awake() {
        //    for (int i = 0; i < toggles.Count; i++) {
        //        toggles[i].OnNotifyToggleGroup -= NotifyToggleStateChanged;
        //        toggles[i].OnNotifyToggleGroup += NotifyToggleStateChanged;
        //    }
        //}

        // Registers given toggle for handling
        public void RegisterToggle(UIToggle toggle) {
            if (!toggles.Contains(toggle))
                toggles.Add(toggle);
        }

        // Toggle will not be handled by this toggle group anymore
        public void UnregisterToggle(UIToggle toggle) {
            if (toggles.Contains(toggle))
                toggles.Remove(toggle);
        }

        // Notify the toggle group about the toggle state change
        public void NotifyToggleStateChanged(UIToggle toggle, bool value) {
            if (isBusy)
                return;

            isBusy = true;
            for (int i = toggles.Count - 1; i >= 0; i--) {
                if (toggle.ToggleGroup != this) {
                    UnregisterToggle(toggle);
                    continue;
                }

                if (toggles[i] == toggle)
                    continue;

                toggles[i].IsOn = value ? false : toggles[i].InitialIsOn;
            }

            activeToggles.Clear();
            for (int i = 0; i < toggles.Count; i++)
                if (toggles[i].IsOn)
                    activeToggles.Add(toggles[i]);

            isBusy = false;
        }

        public List<UIToggle> GetActiveToggles() {
            return new List<UIToggle>(activeToggles);
        }
    }
}
