using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Colosseum.UI {
    [System.Serializable] public class OnSlotInteraction : UnityEvent<Slot> { }

    // todo: move to enums?
    public enum SlotState {
        Empty,
        Locked,
        Filled
    }

    // TODO: Might also want to make use of ISelectable if we 
    // would move characters from slot to slot by clickin on them
    public class Slot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
        public bool isInteractable = true;
        public Sprite lockImage;
        public Image image;
        public OnSlotInteraction onLeftPointerClick;
        public OnSlotInteraction onRightPointerClick;

        [SerializeField] private SlotState state;

        /// <summary>
        /// The assigned slot content.
        /// </summary>
        public SlotContent Content { get; private set; }

        /// <summary>
        /// The current state of the slot.
        /// </summary>
        public SlotState State { get { return state; } }

        // Assign content to the slot
        public bool Assign(SlotContent content) {
            if (state == SlotState.Locked)
                return false;

            Content = content;
            state = SlotState.Filled;

            Refresh();

            return true;
        }

        // Refersh the slot
        public void Refresh() {
            if (state == SlotState.Empty) {
                if (image.enabled)
                    image.enabled = false;
            }
            else if (state == SlotState.Filled) {
                if (!image.enabled)
                    image.enabled = true;
                
                image.sprite = Content.Image;
            }
            else if (state == SlotState.Locked) {
                if (!image.enabled)
                    image.enabled = true;

                image.sprite = lockImage;
            }
        }

        // Remove all content from the slot
        public void Clear() {
            Content = null;
            image.sprite = null;
            state = SlotState.Empty;

            Refresh();
        }

        // Lock the slot
        public void Lock() {
            if (state == SlotState.Filled)
                Clear();

            state = SlotState.Locked;
            Refresh();
        }

        // Unlock the slot
        public void Unlock() {
            state = SlotState.Empty;
            image.sprite = null;
            Refresh();
        }

        // Click handling
        public void OnPointerClick(PointerEventData eventData) {
            if (state == SlotState.Locked || !isInteractable)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
                onLeftPointerClick.Invoke(this);

            if (eventData.button == PointerEventData.InputButton.Right)
                onRightPointerClick.Invoke(this);
        }

        // Click handling
        public void OnPointerDown(PointerEventData eventData) {
        }

        // Click handling
        public void OnPointerUp(PointerEventData eventData) {
        }

        // Old
        //public SlotEvent onAssign = new SlotEvent();
        //public SlotEvent onValidate = new SlotEvent();
        //public SlotEvent onClear = new SlotEvent();

        //private ISelectable lastSelection;
        //private ISelectable assigneeSelection; // assigned selection

        //public bool IsNotifiable {
        //    get {
        //        return gameObject.activeInHierarchy;
        //    }
        //}

        //public bool IsEmpty {
        //    get {
        //        return assigneeSelection == null;
        //    }
        //}


        //public void OnReceiveSelection(ISelectable selection) {
        //    lastSelection = selection;
        //}

        //public void OnReceiveDeselection(ISelectable selection) {
        //    if (selection == lastSelection)
        //        lastSelection = null; // The selection was deselected. Reset reference.
        //}

        //public void Refresh() {
        //    if (assigneeSelection == null)
        //        return;

        //    if (!onValidate.Invoke(assigneeSelection)) {
        //        Clear();
        //        Debug.Log("Slot is invalid, Clearing");
        //        return;
        //    }

        //    image.sprite = assigneeSelection.Image;
        //}

        //public void Clear() {
        //    onClear.Invoke(assigneeSelection);
        //    image.sprite = null;
        //    assigneeSelection = null;
        //    image.enabled = false;
        //}

        //public override void Initialize(object data) {
        //    base.Initialize(data);
        //}

        //[SerializeField] private OnSlotInteraction onClickMouseLeft = new OnSlotInteraction();
        //[SerializeField] private OnSlotInteraction onClickMouseRight = new OnSlotInteraction();

        ///// <summary> Invokes when user is clicking on this slot with left mouse button. </summary>
        //public OnSlotInteraction OnClickMouseLeft { get { return onClickMouseLeft; } }
        ///// <summary> Invokes when user is clicking on this slot with right mouse button. </summary>
        //public OnSlotInteraction OnClickMouseRight { get { return onClickMouseRight; } }


        ///// <summary> Checks whether slot is empty. </summary>
        //public virtual bool IsEmpty() {
        //    return data == null;
        //}

        //// Used to retrieve the data image
        //// All slot childs must implement this in order to make slot working
        //protected abstract Sprite GetImage();

        //protected virtual void OnMousePointerUp(PointerEventData eventData) { }

        ///// <summary> Refreshes slot image </summary>
        //public override void Refresh() {
        //    if (IsEmpty()) {
        //        Clear();
        //        return;
        //    }

        //    image.sprite = GetImage();
        //}

        ///// <summary> Returns the slot to a (visually) blank slate state. </summary>
        //public override void Clear() {
        //    base.Clear();
        //    image.sprite = null;
        //}

        // Invokes pointer events based on with what button the slot was clicked
        //public void OnPointerClick(PointerEventData eventData) {
        //    if (eventData.button == PointerEventData.InputButton.Left) {
        //        onClickMouseLeft.Invoke(this);
        //    }

        //    if (eventData.button == PointerEventData.InputButton.Right) {
        //        onClickMouseRight.Invoke(this);
        //    }

        //    OnMousePointerUp(eventData);
        //}


    }

    // TODO: move to own file
    public class SlotContent {
        /// <summary>
        /// The data of a slot
        /// </summary>
        public object Key { get; private set; }

        /// <summary>
        /// Visual representation of a slot
        /// </summary>
        public Sprite Image { get; private set; }

        public SlotContent(object key, Sprite image) {
            Key = key;
            Image = image;
        }


    }


}