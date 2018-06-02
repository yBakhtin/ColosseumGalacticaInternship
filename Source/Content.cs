using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Colosseum.UI {
    //TODO:
    //public class ContentLayoutElement : MonoBehaviour {
    //    public Element element;
    //    public int preferedIndex;
    //}

    // renamed ElementList to Content
    /// <summary>
    /// Represents collection of elements that is updated based on reference data list.
    /// </summary>
    public class Content : MonoBehaviour /*IList*/ {
        /// <summary>
        /// The prefab the content uses to create elements.
        /// </summary>
        public Element elementPrefab;

        public IList dataList; // only used as refrennce
        //private List<object> overrideList = new List<object>(); // will be used so we can make changes to list (e.g sorting)

        /// <summary>
        /// the raw list of elements within content.
        /// </summary>
        /*[HideInInspector] */public List<Element> elementList = new List<Element>();

        /// <summary>
        /// Is element selection allowed?
        /// </summary>
        public bool selectionAllowed;

        public bool deselectionAllowed = true;
        public bool orderBySiblingIndex;

        /// <summary>
        /// The sorting order of the content (1/-1)
        /// </summary>
        //public int sortOrder = 1; // Do modify variable! Use SwapSortOrder()

        //public bool initialSortingEnabled = true;

        /// <summary>
        /// Invokes when content is expanded.
        /// </summary>
        [Header("Events"), Space]
        public ContentExpandEvent onExpand; // Runtime creation

        /// <summary>
        /// Invokes when one of content elements was clicked.
        /// </summary>
        public ContentEvent onClickElement;

        /// <summary>
        /// Invokes when one of content elements was selected.
        /// </summary>
        public ContentEvent onSelectElement;

        /// <summary>
        /// Invokes when one of content elements was deselected.
        /// </summary>
        public ContentEvent onDeselectElement;

        private Element selection;
        //private System.Comparison<object> comparison;
        //private System.Comparison<object> inversedComparison;
        //private System.Comparison<object> currentComparison;

        /// <summary>
        /// The number of (non-empty) elements of the content.
        /// </summary>
        public int Count { get { return dataList.Count; } }

        /// <summary>
        /// The number of all (including empty) elements in the content.
        /// </summary>
        public int Capacity { get { return elementList.Count; } }

        /// <summary>
        /// Is content initialized?
        /// </summary>
        public bool IsInitialized { get { return dataList != null; } }

        /// <summary>
        /// Is content empty?
        /// </summary>
        public bool IsEmpty { get { return Count == 0; } }

        public bool IsDisplayed { get; private set; }

        //public bool sortingEnabled { get { return initialSortingEnabled && comparison != null; } } // FIXME:TODO:
        
        // Override order in which elements will read the data
        //public System.Comparison<Element> overrideRefreshOrder { get; set; }
        //public ElementOrderDelegate defaultRefreshOrder {
        //    get { } }

        /// <summary>
        /// Current content selection.
        /// </summary>
        public Element Selection { get { return selection; } set { Select(value); } }

        /// <summary>
        /// Sets up content events.
        /// </summary>
        public void SetUp() {
            ForEach(SetUpElement);
        }

        /// <summary>
        /// Initialize content with reference data list.
        /// </summary>
        /// <param name="dataList"> The data list that content will reference when updating </param>
        /// <param name="directRefresh"> Will content refresh directly after initialize? </param>
        public void Initialize(IList dataList, bool directRefresh = false) {
            this.dataList = dataList;

            if (directRefresh)
                Refresh();
        }

        // !!!TODO: Resorting still not working properly, smh...
        /// <summary>
        /// Refreshes the content with all its elements.
        /// </summary>
        public void Refresh() {
            if (!IsInitialized)
                return;

            int range = dataList.Count - elementList.Count;
            if (range > 0)
                Expand(range);

            //List<Element> overrideElementList = new List<Element>(elementList);
            //RefreshOverrideList();
            //Debug.Assert(overrideList.Count != elementList.Count);

            //if (overrideRefreshOrder != null)
            //    overrideElementList.Sort(overrideRefreshOrder);


            //for (int i = 0; i < dataList.Count; i++) {
            //    if (elementList[i].data == dataList[i]) {
            //        Debug.Log("Refresh " + elementList[i].name);
            //        elementList[i].Refresh();
            //        continue;
            //    }

            //    Debug.Log("Init " + elementList[i].name);
            //    elementList[i].Initialize(dataList[i]);
            //    elementList[i].Show();
            //}

            //for (int i = dataList.Count; i < elementList.Count; i++) {
            //    if (elementList[i].IsEmpty)
            //        continue;

            //    Debug.Log("Clear " + elementList[i].name);
            //    elementList[i].Hide();
            //    elementList[i].Clear();
            //}

            int elementCounter = 0;
            for (int i = 0; i < elementList.Count; i++) {
                Element current = elementList[i];

                if (i < dataList.Count && dataList[i] != null) {
                    object currentData = dataList[i];

                    if (currentData == current.data) {
                        current.Refresh();
                    }
                    else {
                        current.Initialize(currentData);
                        current.Show();

                        if (current == selection) // Reselect
                            Select(selection);
                    }

                    if (!deselectionAllowed && selection == null)
                        Select(current);
                }
                else {
                    current.Hide();

                    if (!current.IsEmpty) {
                        if (current == Selection) {
                            if (i == 0)
                                ClearSelection();
                            else
                                Select(elementList[i - 1]);
                        }

                        current.Clear();
                    }
                }

                if (!current.IsEmpty)
                    elementCounter++;
            }
        }

        // TODO: fix name, Add expl
        
        // FIXME: Not the best solution, will remove later. We should rather work with elements instead of data
        //private void RefreshOverrideList() {
        //    //if (overrideRefresh != null)
        //    //    overrideRefresh.Invoke(overrideList, dataList);
        //    //else {

        //    //}

        //    overrideList.Clear();

        //    for (int i = 0; i < dataList.Count; i++) {
        //        overrideList.Add(dataList[i]);
        //    }

        //    if (sortingEnabled)
        //        overrideList.Sort(currentComparison);
        //}

        /// <summary>
        /// Clears the content.
        /// </summary>
        public void Clear() {
            ForEach(ClearElement);
            dataList = null;
        }

        /// <summary>
        /// Hides all content elements.
        /// </summary>
        public void Hide() {
            ForEach(HideElement);
            IsDisplayed = false;
        }

        /// <summary>
        /// Shows all non empty content elements.
        /// </summary>
        public void Show() {
            Refresh();
            ForEach(ShowElement);
            IsDisplayed = true;
        }

        /// <summary>
        /// Creates (N) amount of new elements at the end of content.
        /// </summary>
        /// <param name="range"> The amount of elements that will be created. </param>
        public void Expand(int range) {
            if (range <= 0)
                return;

            if (elementPrefab == null) {
                Debug.LogWarning("Content cannot be further expanded. No prefab specified");
                return;
            }

            List<Element> newElements = new List<Element>();
            elementList.Capacity += range;
            for (int i = 0; i < range; i++)
                newElements.Add(CreateElement());

            onExpand.Invoke(newElements);
        }

        /// <summary>
        /// Select specific element within content.
        /// </summary>
        /// <param name="element"></param>
        public void Select(Element element) {
            if (element == null || element.IsEmpty)
                return;
            if (!elementList.Contains(element))
                return;

            if (selection != null)
                selection.HideSelection();

            selection = element;
            selection.ShowSelection();
            onSelectElement.Invoke(selection);
        }

        /// <summary>
        /// Clears current element selection.
        /// </summary>
        public void ClearSelection() {
            if (selection == null)
                return;

            onDeselectElement.Invoke(selection);
            selection.HideSelection();
            selection = null;
        }

        public void SelectFirstElement() {
            if (elementList.Count == 0)
                return;

            Select(elementList[0]);
        }

        //public void SetSortComparison<TData>(System.Comparison<TData> comparison) {
        //    this.comparison = (x,y) => comparison.Invoke((TData)x, (TData)y);
        //    inversedComparison = (x, y) => comparison.Invoke((TData)y, (TData)x);
        //    currentComparison = this.comparison;
        //}

        //public void SwapSortOrder() {
        //    if (!sortingEnabled)
        //        return;

        //    sortOrder = -sortOrder;
        //    currentComparison = currentComparison == comparison ? inversedComparison : comparison;
        //}

        public int IndexOf(Element element) {
            return elementList.IndexOf(element);
        }

        /// <summary>
        /// Accesses an element at specific index.
        /// </summary>
        /// <param name="index"> The index of element. </param>
        /// <returns></returns>
        public Element this[int index] {
            get { return elementList[index]; }
            set { elementList[index] = value; }
        }

        //private void OnEnable() {
        //    RecreateElementList();
        //}

        //private void OnValidate() {
        //    RecreateElementList();
        //}

        // FIXME: REMOVE
        //private void RecreateElementList() {
        //    Element[] elements = GetComponentsInChildren<Element>();
        //    elementList.Clear();
        //    for (int i = 0; i < elements.Length; i++) {
        //        // Hack: we allow in management that elements are not directly a child of content. (Clinic pods are in sub gameobjects front/back-row)
        //        if((GameManager.Instance.currentPhase != GameManager.GamePhase.Management && elements[i].transform.parent == transform) 
        //            || GameManager.Instance.currentPhase == GameManager.GamePhase.Management) {
        //            elementList.Add(elements[i]);
                    
        //            // NOTE: Missing setup call for existing elements.
        //            SetUpElement(elements[i]);
        //        }
        //    }

        //    if (orderBySiblingIndex) {
        //        elementList.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
        //    }
        //}

        /// <summary>
        /// Set up the element.
        /// </summary>
        /// <param name="element"></param>
        private void SetUpElement(Element element) {
            //element.SetUp();

            if (element.baseSelectionButton != null)
                element.baseSelectionButton.onClick.AddListener(
                    () => {
                        bool isDeselect = false;
                        if (deselectionAllowed && Selection == element) {
                            if (selection == element) {
                                ClearSelection();
                                isDeselect = true;
                            }
                        }

                        if (selectionAllowed && !isDeselect)
                            Select(element);
                        
                        // TODO: What about when deselecting, do we still invoke this?
                        onClickElement.Invoke(element);
                    });
        }

        /// <summary>
        /// Shows the content element.
        /// </summary>
        /// <param name="element"> The inactive non empty element of this content. </param>
        private void ShowElement(Element element) {
            if (!element.IsEmpty && !element.gameObject.activeSelf)
                element.Show();
        }

        /// <summary>
        /// Hides the content element.
        /// </summary>
        /// <param name="element"> The active element of this content. </param>
        private void HideElement(Element element) {
            //Debug.Log(element.gameObject.activeSelf);
            if (element.gameObject.activeSelf) {
                element.Hide();
            }
        }

        /// <summary>
        /// Clears the content element.
        /// </summary>
        /// <param name="element"> The element that will be cleared. </param>
        private void ClearElement(Element element) {
            if (element.IsEmpty)
                return;

            element.Clear();
        }

        /// <summary>
        /// Instantiates an element and places it at the end of the content list. 
        /// </summary>
        /// <returns></returns>
        private Element CreateElement() {
            Element element = Instantiate(elementPrefab, transform, false);
            element.Hide();
            elementList.Add(element);
            SetUpElement(element);
            return element;
        }

        /// <summary>
        /// For each content element helper.
        /// </summary>
        /// <param name="action"></param>
        private void ForEach(System.Action<Element> action) {
            for (int i = 0; i < elementList.Count; i++)
                action(elementList[i]);
        }


        /// <summary>
        /// Describes an content element interaction.
        /// </summary>
        [System.Serializable] public class ContentEvent : UnityEvent<Element> { }

        /// <summary>
        /// Describes the event when content is expanded.
        /// </summary>
        [System.Serializable] public class ContentExpandEvent : UnityEvent<List<Element>> { }
        //public delegate int StubDelegate(Element element);
    }

}

