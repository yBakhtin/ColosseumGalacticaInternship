using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Colosseum.UI {
    public class Foldout : Selectable, IPointerClickHandler {
        public RectTransform content;
        public bool isFolded;
        public UnityEvent onFolded;

        public bool IsFolded {
            get {
                return isFolded;
            }
            set {
                if (content && content.childCount > 0) {
                    onFolded.Invoke();
                    content.gameObject.SetActive(!value);
                }

                isFolded = value;
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            IsFolded = !IsFolded;
        }

        protected override void Awake() {
            base.Awake();
            IsFolded = isFolded;
        }


    }


}