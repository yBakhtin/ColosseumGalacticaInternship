using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Colosseum.Management.UI;
using UnityEngine.EventSystems;

namespace Colosseum.UI {
    public enum SortMode {
        Alphabetical,
        Stats,
        PlayerInteractionFrequency,
        OwnershipPeriod,
        Location
    }

    public enum InventoryViewCategory {
        Gladiators,
        Monsters, // Not implemented
        Weapons,
        Manipulators,
        ShowEnhancers
    }

    // This event is used for when a profile from the gladiatorpool gets selected and deselected
    //[System.Serializable] public class OnProfileInteraction : UnityEvent<HumanoidFighter> { };
    //[System.Serializable] public class FighterProfileEvent : UnityEvent<FighterProfileElement> { }

    // FIXME: Nameing
    [System.Serializable] public class InventoryEvent : UnityEvent<object> { }

    // FIXME: Do not hide inventory view object, otherwise Singelton will not work.
    // TODO: Select fighter when dragging begins, SetUp window releies on it.
    public class InventoryView : SingletonInstance<InventoryView> {
        public ScrollRect scrollRect;

        public Tab fighterTab;
        public Tab weaponTab;
        public Tab showEnhancerTab;
        public Tab manipulatorTab;
        //public Tab drugTab;

        public SortMode currentSortMode;
        public Button alphabeticalSortButton;

        [Header("Events"), Space]
        public InventoryEvent onSelect;
        public InventoryEvent onDeselect;

        //[Space, Header("VFX"), Space]
        //// TODO: Move to ElementTab
        //public bool isSortVfxEnabled = true; 
        //[Range(0.01F, 1)] public float sortVfxDuration = 0.5F;

        [Header("TEMP/DEBUG")]
        //public Sprite drugTestImage;
        public Sprite manipulatorTestImage;
        public Sprite showEnhancerTestImage;

        private Tab activeTab;
        private bool isInitialized;

        public Element Selection { get; private set; }
        protected InventoryManager Inventory { get { return InventoryManager.Instance; } }

        public enum InventoryTab {
            FIGHTER,
            WEAPON,
            SHOW_ENHANCER,
            MANIPULATOR,
            //DRUGS
        }

        private Tab StupidTabLookup(InventoryTab tab) {
            if (tab == InventoryTab.FIGHTER)
                return fighterTab;
            //if (tab == InventoryTab.DRUGS)
            //    return drugTab;
            if (tab == InventoryTab.MANIPULATOR)
                return manipulatorTab;
            if (tab == InventoryTab.SHOW_ENHANCER)
                return showEnhancerTab;
            if (tab == InventoryTab.WEAPON)
                return weaponTab;

            return fighterTab;
        }

        public void OpenIfNot(InventoryTab tab) {
            Tab foundtab = StupidTabLookup(tab);

            if (foundtab != activeTab) {
                 //activeTab.HideContent();
                //activeTab.toggle.isOn = false;
                foundtab.toggle.isOn = true;
            }
        }

        public FighterProfileElement GetFighterProfileElement(HumanoidFighter fromFighter) {
            foreach(FighterProfileElement element in fighterTab.content.elementList)
                if(element.data == fromFighter)
                    return element;

            return null;
        }

        public void SetUp() {
            //alphabeticalSortButton.onClick.AddListener(() => Sort(SortMode.Alphabetical));
            SetUpTab(fighterTab);
            //fighterTab.content.SetSortComparison<HumanoidFighter>(CompareFighters);
            SetUpTab(weaponTab);
            //weaponTab.content.SetSortComparison<Weapon>(CompareWeapons);
            SetUpTab(showEnhancerTab);
            //showEnhancerTab.content.SetSortComparison<ShowEnhancer>(CompareShowEnhancers);
            SetUpTab(manipulatorTab);
            //manipulatorTab.content.SetSortComparison<AudienceManipulator>(CompareAudienceManipulators);
            //SetUpTab(drugTab);
           // drugTab.content.SetSortComparison<Management.Drug>(CompareDrugs);
        }

        // FIXME: No argument because we are using manager.
        private void Initialize() {
            fighterTab.content.Initialize(Inventory.ownHumanoids);
            weaponTab.content.Initialize(Inventory.ownWeapons);
            showEnhancerTab.content.Initialize(Inventory.ownShowEnhancers);
            manipulatorTab.content.Initialize(Inventory.ownManipulators);
            //drugTab.content.Initialize(Inventory.ownDrugs);
            fighterTab.toggle.isOn = true;

            //fighterTabContent.Sort(SortMode.Alphabetical, false);
            //weaponTabContent.Sort(SortMode.Alphabetical, false);
            //showEnhancerTabContent.Sort(SortMode.Alphabetical, false);
            //manipulatorsTabContent.Sort(SortMode.Alphabetical, false);
            //drugTabContent.Sort(SortMode.Alphabetical, false);
        }

        public void Refresh() {
            if (!isInitialized)
                Initialize();

            if (activeTab != null)
                activeTab.content.Refresh();
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void ClearSelection() {
            if (activeTab.content.Selection != null) {
                onDeselect.Invoke(activeTab.content.Selection.data);
                activeTab.content.ClearSelection();
            }

            //fighterTab.content.ClearSelection();
            //weaponTab.content.ClearSelection();
            //showEnhancerTab.content.ClearSelection();
            //manipulatorTab.content.ClearSelection();
            //drugTab.content.ClearSelection();
        }

        public void SetActiveTab(Tab tab) {
            activeTab = tab;
            scrollRect.content = activeTab.content.transform as RectTransform;
            activeTab.ShowContent();
        }

        #region Events
        // Selects clicked element and deselects others
        private void OnSelectElement(Element element, Content content) {
            if (fighterTab.content != content)
                fighterTab.content.ClearSelection();
            if (weaponTab.content != content)
                weaponTab.content.ClearSelection();
            if (showEnhancerTab.content != content)
                showEnhancerTab.content.ClearSelection();
            if (manipulatorTab.content != content)
                manipulatorTab.content.ClearSelection();
           // if (drugTab.content != content)
            //    drugTab.content.ClearSelection();

            Selection = element;
            onSelect.Invoke(element);
        }

        private void OnDeselectElement(Element element) {
            onDeselect.Invoke(element);
            Selection = null;
        }

        private void OnTabToggleValueChanged(bool value, Tab tab) {
            if (!value) {
                tab.HideContent();
                return;
            }

            //if (!value) {
            //    Debug.Log("off");
            //    tab.HideContent();
            //    return;
            //}

            //if (activeTab != null) {
            //    activeTab.HideContent();

            //    if (activeTab == tab) {
            //        activeTab = null;
            //        return;
            //    }
            //}

            SetActiveTab(tab);
        }

        private void SetUpTab(Tab tab) {
            tab.content.onSelectElement.AddListener(element => OnSelectElement(element, tab.content));
            tab.content.onDeselectElement.AddListener(OnDeselectElement);
            tab.toggle.onValueChanged.AddListener(value => OnTabToggleValueChanged(value, tab));
        }

        //private void Sort(SortMode mode) {
        //    if (activeTab == null)
        //        return;

        //    currentSortMode = mode; // Not using yet

        //    activeTab.content.SwapSortOrder();
        //    activeTab.content.Refresh(); // Might also want to use main Refresh()?
        //}

        //int CompareFighters(HumanoidFighter fighterA, HumanoidFighter fighterB) {
        //    return fighterA.fighterName.CompareTo(fighterB.fighterName);
        //}

        //int CompareWeapons(Weapon weaponA, Weapon weaponB) {
        //    return weaponA.name.CompareTo(weaponB.name);
        //}

        //int CompareShowEnhancers(ShowEnhancer showEnhacerA, ShowEnhancer showEnhacerB) {
        //    return showEnhacerA.name.CompareTo(showEnhacerB.name);
        //}

        //int CompareAudienceManipulators(AudienceManipulator manipulatorA, AudienceManipulator manipulatorB) {
        //    return manipulatorA.name.CompareTo(manipulatorB.name);
        //}

        //int CompareDrugs(Management.Drug drugA, Management.Drug drugB) {
        //    return drugA.drugName.CompareTo(drugB.drugName);
        //}

        //private int CompareElements(Element elementA, Element elementB, SortMode mode, int order) {
        //    if (elementA == null && elementB == null)
        //        return 0;

        //    IComparableElement comparableElementA = elementA as IComparableElement;
        //    IComparableElement comparableElementB = elementB as IComparableElement;

        //    if (comparableElementA == null && comparableElementB == null)
        //        return 0; // Both elements are not supported

        //    if (comparableElementA == null)
        //        return comparableElementB.CompareTo(elementA, mode, order);

        //    //if (comparableElementB == null)
        //    //    return comparableElementA.CompareTo(elementB, mode);

        //    return comparableElementA.CompareTo(elementB, mode, order);
        //}


        //public void SetCurrentSelection(Element element) {
        //    fighterTab.content.ClearSelection();
        //    weaponTab.content.ClearSelection();
        //    showEnhancerTab.content.ClearSelection();
        //    manipulatorTab.content.ClearSelection();
        //    drugTab.content.ClearSelection();

        //    //activeTab.content.Select(element);
        //    Selection = element;
        //    onSelect.Invoke(activeTab.content.Selection.data);
        //}


        //private void OnCreateTabElement(Element element) {
        //    element.baseSelectionButton.onClick.AddListener(() => OnClickElement(element));
        //}

        //private void OnClickElement(Element element) {
        //    if (selection != null) {
        //        bool isDeselection = false;

        //        if (selection == element)
        //            isDeselection = true;

        //        ClearSelection();

        //        if (isDeselection)
        //            return;
        //    }

        //    Select(element);
        //}

        //private void OnClickTabActivator(TabContent activatorTab, bool isActive) {
        //    if (isActive) {
        //        activatorTab.tabLayoutElement.flexibleWidth = 1;
        //        scrollRect.content = activatorTab.transform as RectTransform;
        //        activeTab = activatorTab;
        //        activeTab.Show();
        //    }
        //    else {
        //        activatorTab.tabLayoutElement.flexibleWidth = -1; // Should disable
        //        activeTab.Hide();
        //        ClearSelection();
        //    }
        //}

        //private void OnTabSortVfxComplete(TabContent tab) {
        //    scrollRect.verticalScrollbar.interactable = true;
        //}

        #endregion

        //private void SetUpTabContent(TabContent tabContent) {
        //    tabContent.tab.onValueChanged.AddListener(show => OnClickTabActivator(tabContent, show));
        //    tabContent.onCreateElement.AddListener(OnCreateTabElement);
        //}

        //[Header("Prefab properties")]
        //[SerializeField] private FighterProfileElement profilePrefab;

        //[Space, Header("Local properties")]
        //[SerializeField] private RectTransform profileHolder;
        //[SerializeField] private List<FighterProfileElement> profiles;
        //[SerializeField] private Button alphabeticalSortButton;
        //[SerializeField] private Button statSortButton;
        //[SerializeField] private Button playerInteractionFrequencySortButton;
        //[SerializeField] private Button ownershipPeriodSortButton;
        //[SerializeField] private Button locationSortButton;

        //[Space, Header("Events")]
        //[SerializeField] private OnProfileInteraction onProfileSelection = new OnProfileInteraction();
        //[SerializeField] private OnProfileInteraction onProfileDeselection = new OnProfileInteraction();
        //[SerializeField] private FighterProfileEvent onRefreshProfile;
        //[SerializeField] private FighterProfileEvent onCreateProfile;

        //private FighterProfileElement profileSelection;

        //public FighterProfileEvent OnRefreshProfile { get { return onRefreshProfile; } }

        ///// <summary>
        ///// The event is fired when a profile is created.
        ///// </summary>
        //public FighterProfileEvent OnCreateProfile { get { return onCreateProfile; } }
        //public HumanoidFighter FighterSelection {
        //    get {
        //        return ProfileSelectionData;
        //    }
        //}
        //private HumanoidFighter ProfileSelectionData { get; set; }
        //private bool IsAscendingOrdered { get; set; }
        //private SortMode CurrentSortMode { get; set; }
        //private Button LastClickSortButton { get; set; }


        //public void SetUp() {
        //    for (int i = 0; i < profiles.Count; i++) {
        //        FighterProfileElement profile = profiles[i];

        //        profile.SelectionButton.onClick.AddListener(delegate { OnClickProfile(profile); });
        //        profile.FireButton.onClick.AddListener(delegate { FireFighter((HumanoidFighter)profile.data); });
        //    }

        //    ownershipPeriodSortButton.onClick.AddListener(delegate { Sort(SortMode.OwnershipPeriod, ownershipPeriodSortButton); });
        //    alphabeticalSortButton.onClick.AddListener(delegate { Sort(SortMode.Alphabetical, alphabeticalSortButton); });
        //    statSortButton.onClick.AddListener(delegate { Sort(SortMode.Stats, statSortButton); });
        //    playerInteractionFrequencySortButton.onClick.AddListener(delegate { Sort(SortMode.PlayerInteractionFrequency, playerInteractionFrequencySortButton); });
        //    locationSortButton.onClick.AddListener(delegate { Sort(SortMode.Location, locationSortButton); });

        //    Sort(SortMode.Alphabetical, alphabeticalSortButton);
        //}

        //public void Refresh() {
        //    List<HumanoidFighter> inventoryFighters = new List<HumanoidFighter>(Inventory.Instance.ownHumanoids);
        //    inventoryFighters.Sort(GetSortComparison);

        //    int amountDifference = profiles.Count - inventoryFighters.Count;

        //    if (amountDifference < 0)
        //        CreateProfiles(-amountDifference);

        //    for (int i = 0; i < profiles.Count; i++) {
        //        FighterProfileElement profile = profiles[i];
        //        HumanoidFighter profileData = (HumanoidFighter)profile.data;

        //        if (i < inventoryFighters.Count) {
        //            HumanoidFighter inventoryFighter = inventoryFighters[i];

        //            if (inventoryFighter == profileData)
        //                profile.Refresh();
        //            else {
        //                profile.Initialize(inventoryFighter);
        //                profile.gameObject.SetActive(true);
        //            }

        //            OnRefreshProfile.Invoke(profile);
        //        }
        //        else {
        //            profile.Clear();
        //            profile.gameObject.SetActive(false);
        //        }
        //    }

        //    UpdateProfileSelection();
        //}

        //public void CreateProfiles(int amount) {
        //    for (int i = 0; i < amount; i++) {
        //        FighterProfileElement profile = Instantiate(profilePrefab, profileHolder, false);

        //        profile.SelectionButton.onClick.AddListener(delegate { OnClickProfile(profile); });
        //        profile.FireButton.onClick.AddListener(delegate { FireFighter((HumanoidFighter)profile.data); });

        //        profiles.Add(profile);
        //        onCreateProfile.Invoke(profile);
        //    }

        //}

        //public void SelectProfile(FighterProfileElement profile) {
        //    profileSelection = profile;
        //    ProfileSelectionData = (HumanoidFighter)profileSelection.data;
        //    GetProfileSelectionEvent().Invoke((HumanoidFighter)profileSelection.data);
        //    profileSelection.SelectionGraphic.enabled = true;
        //}

        //public void DeselectProfileSelection() {
        //    if (ProfileSelectionData == null)
        //        return;

        //    profileSelection.SelectionGraphic.enabled = false;
        //    GetProfileDeselectionEvent().Invoke((HumanoidFighter)profileSelection.data);
        //    profileSelection = null;
        //    ProfileSelectionData = null;
        //}

        //public List<FighterProfileElement> CloneProfileList() {
        //    return new List<FighterProfileElement>(profiles);
        //}

        //public UnityEvent<HumanoidFighter> GetProfileSelectionEvent() {
        //    return onProfileSelection;
        //}

        //public UnityEvent<HumanoidFighter> GetProfileDeselectionEvent() {
        //    return onProfileDeselection;
        //}

        //private void OnClickProfile(FighterProfileElement profile) {
        //    if (profileSelection == profile)
        //        DeselectProfileSelection();
        //    else {
        //        if (profileSelection != null)
        //            DeselectProfileSelection();

        //        SelectProfile(profile);
        //    }
        //}

        //private void FireFighter(HumanoidFighter profileData) {
        //    if (profileSelection != null && profileData == profileSelection.data)
        //        DeselectProfileSelection();

        //    Inventory.Instance.RemoveFighter(profileData);
        //    Management.UI.UIManagementManager.Instance.CloseAllWindows();
        //    Management.ManagementManager.Instance.ValidateOccupiedFighters();
        //    Refresh();
        //    UpdateProfileSelection();
        //}

        //// Finds and update profile selection graphic corresponding 
        //// to last selection data if selection was changed.
        //private void UpdateProfileSelection() {
        //    if (ProfileSelectionData == null)
        //        return;

        //    if (profileSelection.data != ProfileSelectionData) {
        //        profileSelection.SelectionGraphic.enabled = false;
        //        profileSelection = profiles.Find(profile => profile.data == ProfileSelectionData);

        //        if (profileSelection != null)
        //            profileSelection.SelectionGraphic.enabled = true;
        //    }
        //}

        //private void Sort(SortMode mode, Button button) {
        //    CurrentSortMode = mode;

        //    if (LastClickSortButton == button) {
        //        IsAscendingOrdered = !IsAscendingOrdered;
        //    }

        //    LastClickSortButton = button;
        //    //Refresh();
        //}

        //// Used for profile sorting
        //// TODO: Implement all sorting modes properly at later stage
        //private int GetSortComparison(HumanoidFighter firstFighterData, HumanoidFighter secondFighterData) {
        //    List<HumanoidFighter> fighters = Inventory.instance.ownHumanoids;
        //    int result = -2;
        //    int fallbackResult = -2;
        //    HumanoidFighter firstData = !IsAscendingOrdered ? firstFighterData : secondFighterData;
        //    HumanoidFighter secondData = !IsAscendingOrdered ? secondFighterData : firstFighterData;

        //    // Alphabetical
        //    fallbackResult = firstFighterData.fighterName.CompareTo(secondFighterData.fighterName);

        //    switch (CurrentSortMode) {
        //        case SortMode.Alphabetical:
        //            result = firstData.fighterName.CompareTo(secondData.fighterName);
        //            break;
        //        case SortMode.Stats:
        //            // TODO: Implement real stats compare later
        //            result = secondData.MaxHealth.CompareTo(firstData.MaxHealth);
        //            break;
        //        case SortMode.PlayerInteractionFrequency:
        //            throw new System.NotImplementedException();
        //        //break;
        //        case SortMode.OwnershipPeriod:
        //            throw new System.NotImplementedException();
        //        //break;
        //        case SortMode.Location:
        //            result = firstData.activity.ToString().CompareTo(secondData.activity.ToString());
        //            break;
        //    }

        //    if (result == 0)
        //        result = fallbackResult;

        //    return result;
        //}

        // Old: Page view version
        //public OnProfileInteraction onProfileSelection = new OnProfileInteraction();
        //public OnProfileInteraction onProfileDeselection = new OnProfileInteraction();

        //[Header("GeneralProperties")]
        //[SerializeField] private Text pageField;
        //[SerializeField] private Button nextPageButton;
        //[SerializeField] private Button previousPageButton;
        //[SerializeField] private Dropdown sortModeDropdown;
        //[SerializeField] private Button sortButton;
        //[SerializeField] private bool isAscendingOrdered;
        //[SerializeField] private Button fireButton;

        //[Header("Temp:SelectionVisuals")]
        //[SerializeField] private Graphic selectionVisuals;
        //[SerializeField] private int selectionVisualsSiblingIndex = 1;

        //[Header("ProfileProperties")]
        //[SerializeField] private ElementList profiles;
        //[SerializeField] private FighterProfileEvent onRefreshProfile;

        //private SortMode currentSortMode;
        //private bool isSorted;
        //private int currentPage;
        //private HumanoidFighter profileSelectionData;
        //private FighterProfileElement profileSelection;

        //public int PageCount {
        //    get {
        //        if (profiles.ElementCapacity == 0)
        //            return 0;

        //        return (Inventory.Instance.ownHumanoids.Count - 1) / profiles.ElementCapacity;
        //    }
        //}
        //public HumanoidFighter FighterSelection {
        //    get {
        //        if (profileSelection == null)
        //            return null;

        //        return (HumanoidFighter)profileSelection.data;
        //    }
        //}
        //public FighterProfileEvent OnRefreshProfile {
        //    get {
        //        return onRefreshProfile;
        //    }       
        //}


        //#region PublicGeneralMethods
        //public void SetUp() {
        //    this.SetUpControls();
        //    this.SetUpProfiles();
        //}

        //public void Refresh() {
        //    // FIXME: Could have it's own method
        //    if (this.IsPageEmpty(currentPage) && currentPage > 0)
        //        this.SetCurrentPage(currentPage - 1);

        //    this.RefreshCurrentPageContent();
        //    this.RefreshCurrentPageField();
        //    //this.RefreshSelection();
        //    this.RefreshPageControls();
        //    this.RefreshFireButton();
        //    this.RefreshSortButton();
        //}

        //public UnityEvent<HumanoidFighter> GetProfileSelectionEvent() {
        //    return onProfileSelection;
        //}

        //public UnityEvent<HumanoidFighter> GetProfileDeselectionEvent() {
        //    return onProfileDeselection;
        //}

        //public void DeselectSelection() {
        //    DisableSelectionVisuals();
        //    // Execute any other events before selection is cleared
        //    onProfileDeselection.Invoke(FighterSelection);
        //    profileSelection = null;
        //}

        //public void RefreshSelection() {
        //    if (profileSelectionData == null || 
        //        profileSelection == null)
        //        return;

        //    if (profileSelection.data != profileSelectionData) {
        //        FighterProfileElement newSelection = (FighterProfileElement)profiles.FindElementByData(profileSelectionData);
        //        if (newSelection != null) {
        //            // FIXME: Might need to call SelectProfile instead. (review this)
        //            profileSelection = newSelection;
        //            profileSelectionData = (HumanoidFighter)newSelection.data;
        //            this.ApplySelectionVisuals(profileSelection.transform);
        //        }
        //        else {
        //            this.DeselectSelection();
        //        }
        //    }

        //}

        //public void SelectProfile(FighterProfileElement profile) {
        //    profileSelection = profile;
        //    profileSelectionData = (HumanoidFighter)profile.data;
        //    this.ApplySelectionVisuals(profile.transform);
        //    onProfileSelection.Invoke((HumanoidFighter)profile.data);
        //}

        //public List<FighterProfileElement> BuildFighterProfileList() {
        //    return profiles.ToList().ConvertAll(profile => (FighterProfileElement)profile);
        //}

        //public void DisableControls() {
        //    sortModeDropdown.interactable = false;

        //    for (int i = 0; i < profiles.ElementCapacity; i++)
        //        ((FighterProfileElement)profiles[i]).SelectionButton.interactable = false;
        //}

        //#endregion

        //#region Events
        //// should not be called unless selection != null
        //private void FireFighterSelection() {
        //    HumanoidFighter fighterData = (HumanoidFighter)profileSelection.data;
        //    // FIXME: Temp solution
        //    if (fighterData.activity != FighterActivity.None) {
        //        Debug.LogWarning("Cannot fire selected fighter. He is doing an activity at the moment.");
        //        return;
        //    }

        //    Inventory.Instance.RemoveFighter(fighterData);
        //    this.DeselectSelection();
        //    this.Refresh();
        //}

        //private void OnClickProfile(FighterProfileElement profile) {
        //    if (profileSelection == profile)
        //        this.DeselectSelection();
        //    else
        //        this.SelectProfile(profile);

        //    this.RefreshFireButton();
        //}

        //private void SelectSortMode(int mode) {
        //    currentSortMode = (SortMode)mode;
        //}

        //private void Sort() {
        //    isAscendingOrdered = !isAscendingOrdered;
        //    isSorted = true; // TODO: if later implement unsorted mode, set this flag to somethin like mode != Unsorted
        //    this.RefreshCurrentPageContent();
        //    this.RefreshSelection();
        //}

        //#endregion

        //#region PrivateProfileMethods
        //private void SetUpProfiles() {
        //    List<HumanoidFighter> fighters = Inventory.Instance.ownHumanoids;

        //    // 1. Subscribe to events
        //    for (int i = 0; i < profiles.ElementCapacity; i++) {
        //        FighterProfileElement fighterProfile = (FighterProfileElement)profiles[i];
        //        fighterProfile.SelectionButton.onClick.AddListener(
        //            delegate { OnClickProfile(fighterProfile); });
        //    }

        //    // 2. Set initial profiles state
        //    int profileAmount = Mathf.Clamp(fighters.Count, 0, profiles.ElementCapacity);
        //    for (int i = 0; i < profileAmount; i++) {
        //        profiles[i].Initialize(fighters[i]);
        //        profiles[i].gameObject.SetActive(true);
        //    }
        //}

        //private void RefreshCurrentPageContent() {
        //    List<HumanoidFighter> fighters = new List<HumanoidFighter>(Inventory.Instance.ownHumanoids);

        //    // 1. Sort profiles if needed
        //    if (isSorted)
        //       fighters.Sort(this.GetSortComparison);

        //    int minRange = currentPage * profiles.ElementCapacity;
        //    int maxRange = minRange + profiles.ElementCapacity;
        //    int profileAmount = Mathf.Clamp(fighters.Count, 0, maxRange); // FIXME: Reimpl better formula

        //    // 2. Initialize/Refresh profiles based on current range
        //    for (int i = minRange; i < profileAmount; i++) {
        //        Element profile = profiles[i - minRange];

        //        if (!profile.IsEmpty && profile.data == fighters[i])
        //            profile.Refresh();
        //        else {
        //            profile.Initialize(fighters[i]);
        //            profile.gameObject.SetActive(true);
        //        }

        //        onRefreshProfile.Invoke((FighterProfileElement)profile);
        //    }

        //    // 3. Clear unused profiles
        //    int emptyProfileAmount = profiles.ElementCapacity - (profileAmount - minRange);
        //    for (int i = profileAmount - minRange; i < profiles.ElementCapacity; i++) {
        //        profiles[i].Clear();
        //        profiles[i].gameObject.SetActive(false);
        //    }
        //}

        //// Used by comparing delegate with fighter data for profile sorting
        //// TODO: Implement all sorting modes properly at later stage
        //private int GetSortComparison(HumanoidFighter firstFighterData, HumanoidFighter secondFighterData) {
        //    List<HumanoidFighter> fighters = Inventory.instance.ownHumanoids;
        //    int result = -2;
        //    int fallbackResult = -2;
        //    HumanoidFighter firstData = !isAscendingOrdered ? firstFighterData : secondFighterData;
        //    HumanoidFighter secondData = !isAscendingOrdered ? secondFighterData : firstFighterData;

        //    // Alphabetical
        //    fallbackResult = firstFighterData.fighterName.CompareTo(secondFighterData.fighterName);

        //    switch (currentSortMode) {
        //        case SortMode.Alphabetical:
        //            result = firstData.fighterName.CompareTo(secondData.fighterName);
        //            break;
        //        case SortMode.Stats:
        //            // TODO: Implement real stats compare later
        //            result = secondData.baseHealth.CompareTo(firstData.baseHealth);
        //            break;
        //        case SortMode.PlayerInteractionFrequency:
        //            throw new System.NotImplementedException();
        //        //break;
        //        case SortMode.OwnershipPeriod:
        //            throw new System.NotImplementedException();
        //        //break;
        //        case SortMode.Location:
        //            result = firstData.activity.ToString().CompareTo(secondData.activity.ToString());
        //            break;
        //    }

        //    if (result == 0)
        //        result = fallbackResult;

        //    return result;
        //}
        //#endregion

        //#region PrivateGeneralMethods
        //private void SetUpControls() {
        //    nextPageButton.onClick.AddListener(ToNextPage);
        //    nextPageButton.interactable = currentPage < PageCount;
        //    previousPageButton.onClick.AddListener(ToPreviousPage);
        //    fireButton.onClick.AddListener(FireFighterSelection);
        //    sortModeDropdown.onValueChanged.AddListener(SelectSortMode);
        //    sortButton.onClick.AddListener(Sort);
        //    sortButton.interactable = !(IsPageEmpty(currentPage) && PageCount == 0);
        //}

        //private void RefreshSortButton() {
        //    if (IsPageEmpty(currentPage) && PageCount == 0)
        //        sortButton.interactable = false;
        //    else
        //        sortButton.interactable = true;
        //}

        //private void RefreshFireButton() {
        //    fireButton.interactable = profileSelection != null;
        //}

        //private void RefreshPageControls() {
        //    nextPageButton.interactable = currentPage < PageCount;
        //    previousPageButton.interactable = currentPage > 0;
        //}

        //private void RefreshCurrentPageField() {
        //    pageField.text = (currentPage + 1).ToString();
        //}

        //private void SetCurrentPage(int page) {
        //    currentPage = page;
        //    this.DeselectSelection();
        //}

        //private bool IsPageEmpty(int page) {
        //    return page * profiles.ElementCapacity > Inventory.Instance.FighterCount - 1;
        //}

        //private void ToNextPage() {
        //    this.SetCurrentPage(currentPage + 1);
        //    this.Refresh();
        //}

        //private void ToPreviousPage() {
        //    this.SetCurrentPage(currentPage - 1);
        //    this.Refresh();
        //}

        //private void ApplySelectionVisuals(Transform transform) {
        //    if (!selectionVisuals.gameObject.activeSelf)
        //        selectionVisuals.gameObject.SetActive(true);

        //    selectionVisuals.transform.SetParent(transform, false);
        //    selectionVisuals.transform.SetSiblingIndex(selectionVisualsSiblingIndex);
        //}

        //private void DisableSelectionVisuals() {
        //    if (selectionVisuals.gameObject.activeSelf)
        //        selectionVisuals.gameObject.SetActive(false);
        //}
        //#endregion

        // Legacy
        //[System.Serializable] public class FighterPoolSelection {
        //    [System.NonSerialized] public HumanoidFighter data;
        //    [System.NonSerialized] public FighterProfileElement element;
        //    [System.NonSerialized] public bool isInteractable = true;
        //    [SerializeField] private Graphic visuals;
        //    [SerializeField] private int visualsSiblingIndex;

        //    public bool IsEnabled {
        //        get {
        //            return visuals.enabled;
        //        }
        //    }

        //    //public bool Validate() {
        //    //    if ()
        //    //}

        //    public void Initialize(FighterProfileElement element) {
        //        this.element = element;
        //        data = (HumanoidFighter)this.element.data;
        //    }

        //    public void Clear() {
        //        element = null;
        //        data = null;
        //    }

        //    public void SetFocus(FighterProfileElement element) {
        //        if (!visuals.gameObject.activeSelf)
        //            visuals.gameObject.SetActive(true);

        //        visuals.transform.SetParent(element.transform, false);
        //        visuals.transform.SetSiblingIndex(visualsSiblingIndex);
        //    }

        //    /// <summary>
        //    /// Disable selection visuals.
        //    /// </summary>
        //    public void Disable() {
        //        if (visuals.gameObject.activeSelf)
        //            visuals.gameObject.SetActive(false);
        //    }
        //}

        //private void DisableProfileSelectionVisuals() {
        //    profileSelectionVisuals.enabled = false;
        //}

        //private int FindPageIndexByProfileData(object profileData) {
        //    int index = Inventory.Instance.ownHumanoids.IndexOf((HumanoidFighter)profileData);
        //    return index == -1 ? -1 : index / profiles.ElementCapacity;
        //}

        //public void AddProfile(HumanoidFighter fighter) {
        //    FighterProfileElement emptyProfile = ObtainEmptyProfile();
        //    emptyProfile.Initialize(fighter);
        //    emptyProfile.selectionButton.onClick.AddListener(delegate { SelectProfile(emptyProfile); });
        //    activeProfileCount++;
        //}

        //public bool RemoveProfile(HumanoidFighter fighter) {
        //    FighterProfileElement profile = FindProfile(fighter);
        //    if (profile != null) {
        //        RemoveProfile(profile);
        //        return true;
        //    }

        //    return false;
        //}

        //#endregion

        //public void AllowMultipleSelection(bool value) {
        //    if (!value && isMultipleSelectionAllowed) {
        //        List<FighterProfileElement> profileSelection = GetMultipleProfileSelection();
        //        for (int i = 0; i < profileSelection.Count; i++) {
        //            if (profileSelection[i] != lastProfileSelection)
        //                profileSelection[i].Deselect();
        //        }
        //    }

        //    isMultipleSelectionAllowed = value;
        //}

        //public void SetVisible(bool value) {
        //    gameObject.SetActive(value);
        //}

        //#region Event Listeners
        //private void Sort() {
        //    FighterProfileElement[] activeProfiles = GetActiveProfiles();
        //    HumanoidFighter[] sortedFighters = new HumanoidFighter[activeProfileCount];

        //    sortComparer.mode = startSortMode;
        //    Array.Sort(activeProfiles, sortComparer);
        //    for (int i = 0; i < activeProfileCount; i++) {
        //        sortedFighters[i] = (HumanoidFighter)activeProfiles[i].data;
        //    }

        //    for (int i = 0; i < activeProfiles.Length; i++) {
        //        int tapIndex = i / maxProfileTapCount;
        //        taps[tapIndex].profiles[i - tapIndex * maxProfileTapCount].Initialize(sortedFighters[i]);
        //    }

        //    sortComparer.isSortAscending = !sortComparer.isSortAscending;
        //}
        //private void SelectSortMode(int mode) {
        //    sortComparer.isSortAscending = isSortAscending;
        //    startSortMode = (SortMode)mode;
        //}
        //private void SelectProfile(FighterProfileElement profile) {
        //    onProfileSelection.Invoke((HumanoidFighter)profile.data);

        //    if (profile.IsSelected) {
        //        profile.Deselect();
        //        onProfileDeselection.Invoke((HumanoidFighter)profile.data);
        //        return;
        //    }

        //    if (!isMultipleSelectionAllowed) {
        //        List<FighterProfileElement> profileSelection = GetMultipleProfileSelection();

        //        if (profileSelection.Count > 0)
        //            profileSelection[0].Deselect();
        //    }

        //    profile.Select(profileSelectionColor);
        //    lastProfileSelection = profile;
        //}
        //private void SwitchTap(int direction) {
        //    if (taps.Count == 0)
        //        return;

        //    taps[activeTapIndex].gameObject.SetActive(false);

        //    if (direction != 0) {
        //        if (direction == 1)
        //            activeTapIndex++;
        //        else if (direction == -1)
        //            activeTapIndex--;
        //    }

        //    if (activeTapIndex < 0)
        //        activeTapIndex = 0;

        //    if (!taps[activeTapIndex].gameObject.activeSelf) {
        //        taps[activeTapIndex].Refresh();
        //        taps[activeTapIndex].gameObject.SetActive(true);
        //    }

        //    RefreshTapButtons();
        //    activeTapIndexField.text = "page " + (activeTapIndex + 1).ToString();
        //}
        //private void FireSelectionFighters() {
        //    List<FighterProfileElement> selectionProfiles = GetMultipleProfileSelection();

        //    if (selectionProfiles.Count > 0) {
        //        for (int i = 0; i < selectionProfiles.Count; i++)
        //            Inventory.Instance.RemoveFighter((HumanoidFighter)selectionProfiles[i].data);

        //        sortComparer.isSortAscending = isSortAscending;
        //        Refresh();
        //    }
        //}

        //private void OnClickWeaponSlot(BaseEventData eventData, FighterProfileElement profile) {
        //    PointerEventData pointerEventData = (PointerEventData)eventData;
        //    if (pointerEventData.button == PointerEventData.InputButton.Left) {
        //        if (!profile.IsSelected)
        //            SelectProfile(profile);

        //        Management.UI.UIManagementManager.Instance.ShowWeaponInventory();
        //    }

        //    if (pointerEventData.button == PointerEventData.InputButton.Right) {
        //        HumanoidFighter fighter = (HumanoidFighter)profile.data;
        //        fighter.ChangeWeapon();
        //        profile.Refresh();
        //        Management.UI.UIManagementManager.Instance.ShowWeaponInventory();
        //    }
        //}

        //#endregion

        //#region Private Profile Methods
        //private bool AddMissingProfiles() {
        //    List<HumanoidFighter> fighters = new List<HumanoidFighter>(
        //        Inventory.Instance.ownHumanoids
        //        );

        //    int lastCount = activeProfileCount;
        //    for (int i = 0; i < fighters.Count; i++) {
        //        if (!profiles.Exists(profile => (HumanoidFighter)profile.data == fighters[i])) {
        //            AddProfile(fighters[i]);
        //        }
        //    }

        //    if (lastCount != activeProfileCount)
        //        sortComparer.isSortAscending = isSortAscending;

        //    return lastCount != activeProfileCount;
        //}
        //private bool RemoveInvalidProfiles() {
        //    List<HumanoidFighter> fighters = Inventory.Instance.ownHumanoids;
        //    int lastCount = activeProfileCount;

        //    for (int i = activeProfileCount - 1; i >= 0; i--)
        //        if (!fighters.Contains((HumanoidFighter)profiles[i].data))
        //            RemoveProfile(profiles[i]);

        //    return activeProfileCount != lastCount;
        //}
        //private void RemoveProfile(FighterProfileElement profile) {
        //    FighterProfileElement lastActiveProfile = profiles[activeProfileCount - 1];
        //    SwapProfiles(profile, lastActiveProfile);
        //    ClearProfile(lastActiveProfile);
        //    profile.Deselect();
        //    activeProfileCount--;

        //    if (activeTapIndex > GetActiveTapCount() - 1)
        //        SwitchTap(-1);
        //}
        //private void ClearProfile(FighterProfileElement profile) {
        //    profile.Deselect();
        //    profile.gameObject.SetActive(false);
        //    profile.Clear();
        //}
        //private FighterProfileElement ObtainEmptyProfile() {
        //    // Find first profile that is empty
        //    FighterProfileElement emptyProfile = profiles.Find(profile => profile.IsEmpty);

        //    if (emptyProfile == null) {
        //        FighterProfileTap tap = CreateTap();
        //        emptyProfile = tap.profiles[0];
        //    }

        //    return emptyProfile;
        //}
        //private void SwapProfiles(FighterProfileElement left, FighterProfileElement right) {
        //    HumanoidFighter leftFighter = (HumanoidFighter)left.data;
        //    left.Initialize((HumanoidFighter)right.data);
        //    right.Initialize(leftFighter);

        //    if (left.IsSelected && !right.IsSelected) {
        //        left.Deselect();
        //        right.Select(profileSelectionColor);
        //    }

        //    if (right.IsSelected && !left.IsSelected) {
        //        right.Deselect();
        //        left.Select(profileSelectionColor);
        //    }
        //}
        //private FighterProfileElement[] GetActiveProfiles() {
        //    FighterProfileElement[] activeProfiles = new FighterProfileElement[activeProfileCount];
        //    for (int i = 0; i < activeProfileCount; i++)
        //        activeProfiles[i] = profiles[i];

        //    return activeProfiles;
        //}
        //#endregion

        //private FighterProfileTap CreateTap() {
        //    FighterProfileTap newTap = Instantiate(tapPrefab, tapHolder, false);

        //    for (int i = 0; i < newTap.profiles.Count; i++) {
        //        int index = i;
        //        EventTrigger.Entry entry = new EventTrigger.Entry();
        //        entry.eventID = EventTriggerType.PointerClick;
        //        entry.callback.AddListener(data => { OnClickWeaponSlot(data, newTap.profiles[index]); });
        //        newTap.profiles[i].weaponSlotTrigger.triggers.Add(entry);
        //    }

        //    profiles.AddRange(newTap.profiles);
        //    taps.Add(newTap);
        //    nextTapButton.interactable = true;

        //    return newTap;
        //}

        //// TODO: We could place here all gui/button/label relateted stuff here
        //// Maybe also resetting toggles
        //private void RefreshTapButtons() {
        //    nextTapButton.interactable = activeTapIndex <= GetActiveTapCount() - 2;
        //    previousTapButton.interactable = activeTapIndex > 0;
        //}


        //#region Getters
        //// FIXME: Change return type to HumanoidFighter if sure that profile properties are not needed.
        //public HumanoidFighter GetFighterSelection() {
        //    List<FighterProfileElement> profiles = GetMultipleProfileSelection();
        //    return profiles.Count == 0 ? null : (HumanoidFighter)profiles[0].data;
        //}

        //// FIXME: Change return type to HumanoidFighter[] if sure that profile properties are not needed.
        //public List<FighterProfileElement> GetMultipleProfileSelection() {
        //    return profiles.FindAll(profile => profile.IsSelected);
        //}

        //public FighterProfileElement FindProfile(HumanoidFighter fighterData) {
        //    FighterProfileElement matchingProfile = null;

        //    for (int i = 0; i < profiles.Count; i++) {
        //        if (profiles[i].IsEmpty)
        //            continue;

        //        if (profiles[i].data == fighterData) {
        //            matchingProfile = profiles[i];
        //            break;
        //        }
        //    }

        //    return matchingProfile;
        //}

        //public int GetActiveProfileCount() {
        //    return activeProfileCount;
        //}

        //private int GetActiveTapCount() {
        //    return (activeProfileCount + maxProfileTapCount - 1) / maxProfileTapCount;
        //}
        //#endregion
    }

    //public class FighterProfileSettings {
    //    private FighterProfileElement profile;
    //    private System.Action<FighterProfileElement> action;
    //    private System.Predicate<FighterProfileElement> precondition;

    //    public FighterProfileSettings(FighterProfileElement profile) {
    //        this.profile = profile;
    //    }

    //    public void AddSetting(System.Action<>) {
    //        action += 
    //    }

    //    // might be void aswell
    //    public bool Apply() {
    //        if (precondition(profile)) {
    //            action(profile);
    //            return true;
    //        }

    //        return false;
    //    }
    //}

    // layer
    //public class FighterProfileInfo {
    //    public static int MaxPageProfileAmount { get; set; }
    //    public bool IsProfileSelectable { get; set; }
    //    //public bool IsProfileSelection { get; set; }
    //    public bool IsProfileRemovePending { get; set; }
    //    public int Index { get; /*private*/ set; }

    //    /// <summary>
    //    /// The page on which the profile is located in fighter pool.
    //    /// </summary>
    //    public int Page {
    //        get {
    //            return Index / MaxPageProfileAmount;
    //        }
    //    }

    //    public FighterProfileInfo(int index) {
    //        this.Index = index;
    //        IsProfileSelectable = true;
    //    }
    //}
}