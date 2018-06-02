using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Colosseum.UI;
using Prime31.ZestKit;

namespace Colosseum.Management.UI {
    public class MarketWindow : GenericWindow {
        public ScrollRect itemScrollView;
        public Tab weaponTab;
        public Tab showEnhancerTab;
        //public Tab audienceManipulatorTab;
        //public Tab drugTab;
        public Text moolahField;
        public Button rerollButton;
        public Button alphabeticalSortButton;
        public Image itemPreviewImage;

        private Tab activeTab;
        private Element seleciton;

        // The manager we are working with
        public MarketManager Marketplace { get { return MarketManager.Instance; } }

        // Sets up the whole screen.
        public override void SetUp() {
            base.SetUp();
            //Marketplace.DebugCreateTestMarketplaceInventory();

            SetUpInventoryTab(weaponTab);
            SetUpInventoryTab(showEnhancerTab);
            //SetUpInventoryTab(audienceManipulatorTab);
            //SetUpInventoryTab(drugTab);

            //rerollButton.onClick.AddListener(ReRollInventory);

            weaponTab.content.Initialize(Marketplace.weaponInventory);
            showEnhancerTab.content.Initialize(Marketplace.showEnhancerInventory);
            //audienceManipulatorTab.content.Initialize(Marketplace.audienceManipulatorInventory);
            //drugTab.content.Initialize(Marketplace.drugInventory);

            //weaponTab.content.SetSortComparison<Weapon>(CompareWeapons);
            //showEnhancerTab.content.SetSortComparison<ShowEnhancer>(CompareShowEnhancers);
            //audienceManipulatorTab.content.SetSortComparison<AudienceManipulator>(CompareAudienceManipulators);
            //drugTab.content.SetSortComparison<Management.Drug>(CompareDrugs);

            //alphabeticalSortButton.onClick.AddListener(Sort);
        }

        // Attempts to buy a specified item
        public void BuyItem(MarketItemElement itemElement, Content contextContent) {
            if (Marketplace.BuyItem(itemElement.data, contextContent.dataList, itemElement.GetDataItemPrice())) {
                contextContent.Refresh();
                RefreshMoolahField();
                InventoryView.Instance.Refresh();
                RefreshPreview();
            }
        }

        public override void Open() {
            base.Open();
            weaponTab.content.Refresh();
            //showEnhancerTab.content.Refresh();
            //audienceManipulatorTab.content.Refresh();
            //drugTab.content.Refresh();
            RefreshMoolahField();

            showEnhancerTab.toggle.isOn = false;
            //audienceManipulatorTab.toggle.isOn = false;
            //drugTab.toggle.isOn = false;

            //weaponTab.toggle.isOn = true;
            // Because toggle is not always triggered
            weaponTab.toggle.isOn = true;
            if (activeTab != null)
                activeTab.HideContent();
            SetActiveInventoryTab(weaponTab);
            
            AdvisorManager.Instance.ShowAdvisorText(AdvisorManager.ADVISORS.MARKET, AdvisorManager.Instance.MARKET_ADVISOR_TEXT);
        }

        // Handles switching tabs
        private void OnInventoryTabToggleValueChanged(bool value, Tab contextTab) {
            if (!value) {
                contextTab.HideContent();
                return;
            }

            SetActiveInventoryTab(contextTab);
        }

        // Setup inventory tab for use by player
        private void SetUpInventoryTab(Tab tab) {
            tab.content.SetUp();

            tab.toggle.onValueChanged.AddListener(value => OnInventoryTabToggleValueChanged(value, tab));
            tab.content.onExpand.AddListener(elements => OnExpandInvetnoryTabContent(elements, tab.content));
            tab.content.onSelectElement.AddListener(element => OnSelectItemElement(element, tab.content));
        }

        // Set(Show) current active inventory tab.
        public void SetActiveInventoryTab(Tab tab) {
            if(tab == weaponTab)
                InventoryView.Instance.OpenIfNot(InventoryView.InventoryTab.WEAPON);
            else if(tab == showEnhancerTab)
                InventoryView.Instance.OpenIfNot(InventoryView.InventoryTab.SHOW_ENHANCER);
            //else if(tab == audienceManipulatorTab)
            //    InventoryView.Instance.OpenIfNot(InventoryView.InventoryTab.MANIPULATOR);
            //else if(tab == drugTab)
            //    InventoryView.Instance.OpenIfNot(InventoryView.InventoryTab.DRUGS);
            
            activeTab = tab;
            itemScrollView.content = activeTab.content.transform as RectTransform;
            activeTab.ShowContent();
            activeTab.content.SelectFirstElement();
            RefreshPreview();
        }

        // When new elements are added to the tab we also want to set them up.
        private void OnExpandInvetnoryTabContent(List<Element> newElements, Content contextContent) {
            for (int i = 0; i < newElements.Count; i++) {
                MarketItemElement itemElement = newElements[i] as MarketItemElement;
                SetUpItemElement(itemElement, contextContent);
            }
        }

        // Selects clicked element and deselects others
        private void OnSelectItemElement(Element element, Content contextContent) {
            seleciton = element;

            // FIXME: Weird enough active tab is null at some point.
            // will look into this later
            if (activeTab != null) {
                RefreshPreview();
            }
        }

        // Setup market item element for player interaction
        private void SetUpItemElement(MarketItemElement itemElement, Content contextContent) {
            itemElement.buyButton.onClick.AddListener(() => BuyItem(itemElement, contextContent));
        }

        //// Just to make sure everything is hidden before we start.
        //private void HideAllInventoryTabs() {
        //    weaponTab.HideContent();
        //    showEnhancerTab.HideContent();
        //    audienceManipulatorTab.HideContent();
        //    drugTab.HideContent();
        //}

        // Refresh money 
        private void RefreshMoolahField() {
            moolahField.text = InventoryManager.Instance.Moolah.ToMoneyString() + " moolah";
        }

        // Handle this differently if other stuff will be used for preview. (Perhaps spine or some maya stuff?)
        private void RefreshPreview() {
            if (activeTab.content.IsEmpty) {
                itemPreviewImage.sprite = null;
                itemPreviewImage.enabled = false;
            }
            else {
                itemPreviewImage.enabled = true;
                itemPreviewImage.sprite = ((MarketItemElement)seleciton).portraitImage.sprite;

                ZestKit.instance.stopAllTweensWithTarget(itemPreviewImage.rectTransform, true, true);
                var endPos = itemPreviewImage.rectTransform.anchoredPosition3D;    // Magic garbage.
                var startPos = endPos - Vector3.up * 75.0f;                        // Magic garbage.

                itemPreviewImage.rectTransform.anchoredPosition3D = startPos;

                itemPreviewImage.rectTransform.ZKanchoredPosition3DTo(endPos, 0.4f)
                    .setEaseType(EaseType.CircOut)
                    .start();
            }
        }

        //// Reroll all intentory items and refresh active tab.
        //private void ReRollInventory() {
        //    Marketplace.ReRollInventory();
        //    activeTab.content.Refresh();
        //}

        //private void Sort() {
        //    activeTab.content.SwapSortOrder();
        //    activeTab.content.Refresh();
        //}

        //// Should implement this in a better way
        //#region COPYPASTE FROM INVENTORY VIEW
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
        //#endregion

        // Legacy
        //    private enum Tabs {
        //        none,
        //        gladiator,
        //        gear
        //    }

        //    [Header("Buttons")]
        //    [SerializeField] private Button closeButton;
        //    [SerializeField] private Button gladiatorTabButton;
        //    [SerializeField] private Button gearTabButton;

        //    [Header("Prefab")]
        //    [SerializeField] private MarketFighterElement fighterElementPrefab;
        //    [SerializeField] private MarketGearElement gearElementPrefab;

        //    [Header("Gladiator Tab")]
        //    [SerializeField] private Transform gladiatorTab;
        //    [SerializeField] private Transform gladiatorShelve;

        //    [Header("Gear Tab")]
        //    [SerializeField] private Transform gearTab;
        //    [SerializeField] private Transform weaponShelve;

        //    [Header("General")]
        //    [SerializeField] private Text ownedGoldField;
        //    private Tabs activeTab;


        //    //public void FillFighterShelves() {
        //    //    List<HumanoidFighter> inventory = MarketManager.Instance.fighterInventory;

        //    //    ClearShelve(gladiatorShelve);
        //    //    for (int i = 0; i < inventory.Count; i++) {
        //    //        CreateFighterElement(inventory[i]).transform.SetParent(gladiatorShelve, false);
        //    //    }
        //    //}

        //    public void FillWeaponShelve() {
        //        List<Weapon> inventory = MarketManager.Instance.weaponInventory;

        //        ClearShelve(weaponShelve);
        //        for (int i = 0; i < inventory.Count; i++) {
        //            CreateGearElement(inventory[i]).transform.SetParent(weaponShelve, false);
        //        }
        //    }

        //    public override void Open() {
        //        //FillFighterShelves();
        //        FillWeaponShelve();

        //        RefreshOwnedGoldField();
        //        SwitchTab(Tabs.gear);
        //        base.Open();
        //    }

        //    public override void Close() {
        //        SwitchTab(Tabs.none);
        //        base.Close();
        //    }

        //    protected override void Awake() {
        //        closeButton.onClick.AddListener(Close);
        //        gladiatorTabButton.onClick.AddListener(delegate { SwitchTab(Tabs.gladiator); });
        //        gearTabButton.onClick.AddListener(delegate { SwitchTab(Tabs.gear); });
        //    }

        //    //private MarketFighterElement CreateFighterElement(HumanoidFighter data) {
        //    //    MarketFighterElement fighterElement = Instantiate<MarketFighterElement>(fighterElementPrefab);
        //    //    fighterElement.Initialize(data);
        //    //    fighterElement.buyButton.onClick.AddListener(delegate { BuyFighter(fighterElement); });
        //    //    return fighterElement;
        //    //}

        //    private MarketGearElement CreateGearElement(Gear data) {
        //        MarketGearElement gearElement = Instantiate<MarketGearElement>(gearElementPrefab);
        //        gearElement.Initialize(data);
        //        gearElement.buyButton.onClick.AddListener(delegate { BuyGear(gearElement); } );
        //        return gearElement;
        //    }

        //    private void ClearShelve(Transform shelve) {
        //        for (int i = 0; i < shelve.childCount; i++) {
        //            Destroy(shelve.GetChild(i).gameObject);
        //        }

        //    }

        //    private bool IsBuyable(Fighter fighter) {
        //        if (fighter.price <= Inventory.Instance.Moolah)
        //            return true;
        //        else
        //            return false;
        //    }

        //    private bool IsBuyable(Gear gear) {
        //        if (gear.price <= Inventory.Instance.Moolah)
        //            return true;
        //        else
        //            return false;
        //    }

        //    private void SwitchTab(Tabs newTab) {
        //        if (activeTab == Tabs.gladiator) {
        //            gladiatorTab.gameObject.SetActive(false);
        //            gladiatorTabButton.interactable = true;
        //        }
        //        else if (activeTab == Tabs.gear) {
        //            gearTab.gameObject.SetActive(false);
        //            gearTabButton.interactable = true;
        //        }

        //        switch (newTab) {
        //            case Tabs.none:
        //            activeTab = Tabs.none;
        //            break;
        //            case Tabs.gladiator:
        //            gladiatorTab.gameObject.SetActive(true);
        //            gladiatorTabButton.interactable = false;
        //            activeTab = Tabs.gladiator;
        //            break;
        //            case Tabs.gear:
        //            gearTab.gameObject.SetActive(true);
        //            gearTabButton.interactable = false;
        //            activeTab = Tabs.gear;
        //            break;
        //        }
        //    }

        //    private void RefreshOwnedGoldField() {
        //        ownedGoldField.text = "Gold: " + Inventory.Instance.Moolah.ToString();
        //    }

        //    //private void BuyFighter(ShopFighterElement element) {
        //    //    if (IsBuyable(element.fighter)) {
        //    //        Inventory.Instance.ModifyMoney(-element.fighter.price);
        //    //        RefreshOwnedGoldField();
        //    //        element.Bought();

        //    //        Inventory.Instance.AddHumanoidFighter((HumanoidFighter)element.fighter);
        //    //        MarketManager.Instance.fighterInventory.Remove((HumanoidFighter)element.fighter);
        //    //    }
        //    //}

        //    private void BuyGear(MarketGearElement element) {
        //        if (IsBuyable(element.gear)) {
        //            Inventory.Instance.ModifyMoolah(-element.gear.price);
        //            RefreshOwnedGoldField();
        //            element.Bought();

        //            Inventory.Instance.AddGear(element.gear);
        //            MarketManager.Instance.weaponInventory.Remove((Weapon)element.gear);
        //        }
        //    }
    }
}
