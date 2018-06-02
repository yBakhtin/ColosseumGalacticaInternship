using System.Collections.Generic;
using UnityEngine;
using Colosseum.Management.UI;

namespace Colosseum.Management {
    public class TalentAgencyManager : SingletonInstance<TalentAgencyManager> {
        //public int level = 0;
        public int stockCapacity = 3;
        public int maxStockCapacity = 9;

        public List<HumanoidFighter> displayHumanoids = new List<HumanoidFighter>();
        
        // Hire specific fighter (will be added to the inventory)
        public bool HireFighter(HumanoidFighter humanoidFighter) {
            if (InventoryManager.Instance.Moolah < humanoidFighter.price) {
                Debug.LogWarning("not enough money to hire fighter");
                return false;
            }

            InventoryManager.Instance.ModifyMoolah(-humanoidFighter.price);
            InventoryManager.Instance.AddHumanoidFighter(humanoidFighter);
            displayHumanoids.Remove(humanoidFighter);
            UIManagementManager.Instance.RefreshInventoryView();

            return true;
        }

        // FIXME: Temporarily disabled for the rezzed demo
        //public void ReplenishGladiatorStock() {
        //    offeringFighters.Clear();
        //    offeringFighters.AddRange(FighterGenerator.GenerateHumanoids(stockCapacity));

        //    Debug.Log("<color=green>TalentAgencyManager</color>: gladiator stock has been replenished");
        //}

        // increase the capacity of the stock
        public bool IncreaseStockCapacity() {
            float cost = InventoryManager.Instance.Moolah - 100 * Mathf.Pow(stockCapacity, 2);

            if (stockCapacity == maxStockCapacity) {
                Debug.Log("<color=green>TalentAgencyManager</color>: could not increase stock capacity. " +
                    "Max amount of slots reached");

                return false;
            }

            if ((InventoryManager.Instance.Moolah - cost) < 0) {
                Debug.Log("<color=green>TalentAgencyManager</color>: could not increase stock capacity. " +
                    "Not enough gold availible");

                return false;
            }

            InventoryManager.Instance.ModifyMoolah(cost);
            stockCapacity++;
            return true;
        }


        // Legacy
        //public int scoutSeekDuration = 4; // 4 turns
        //public int scoutSalary = 100; // gold
        //public float rareGladiatorChance = 0.45F; // represented in %
        //public List<Scout> scouts = new List<Scout>();

        // Monthly salary of all scouts together
        //public int Salary { get { return scoutSalary * scouts.Count; } }

        //public int AvailibleScoutCount {
        //    get {
        //        int count = 0;
        //        for (int i = 0; i < scouts.Count; i++) {
        //            if (!scouts[i].isSeeking) {
        //                count++;
        //            }
        //        }

        //        return count;
        //    }
        //}

        //public bool HireScout() {
        //    // TODO: return if stage requirement is not met

        //    Scout scout = new Scout();
        //    scout.name = "Scout_" + scout.GetHashCode();
        //    scouts.Add(scout);

        //    Debug.Log("<color=green>TalentAgencyManager</color>: scout [<color=blue>" + 
        //        scout.name + "</color>] has been hired.");

        //    return true;
        //}

        // seekDuration will be replaced with region
        //public void SendScout(int seekDuration) {
        //    if (scouts.Count == 0) {
        //        Debug.Log("<color=green>TalentAgencyManager</color>: You dont have any hired scouts yet");
        //        return;
        //    }

        //    Scout scout = null;
        //    for (int i = 0; i < scouts.Count; i++) {
        //        if (!scouts[i].isSeeking) {
        //            scouts[i].seekTreshold = seekDuration;
        //            scouts[i].isSeeking = true;
        //            scout = scouts[i];
        //            break;
        //        }
        //    }

        //    if (scout == null) {
        //        Debug.Log("<color=green>TalentAgencyManager</color>: " +
        //            "No scouts are availible for use at the moment, please try again later");

        //        return;
        //    }

        //    Debug.Log("<color=green>TalentAgencyManager</color>: scout [<color=blue>" +
        //        scout.name + "</color>] has been sended for gladiator seek");
        //}

        //public void SeekFighters() {
        //    for (int i = 0; i < scouts.Count; i++) {
        //        if (!scouts[i].isSeeking)
        //            continue;

        //        HumanoidFighter humanoidFighter = scouts[i].SeekGladiator(rareGladiatorChance);

        //        if (humanoidFighter != null) {
        //            if (offeringFighters.Count == stockCapacity) {
        //                Debug.Log("<color=green>TalentAgencyManager</color>: could not register a fighter [<color=blue>"
        //                    + humanoidFighter.fighterName + "</color>]. Not enough slots availible");
        //            }
        //            else {
        //                offeringFighters.Add(humanoidFighter);

        //                Debug.Log("<color=green>TalentAgencyManager</color>: fighter [<color=blue>"
        //                    + humanoidFighter.fighterName + "</color>] added to the talent agency stock");
        //            }

        //            scouts[i].isSeeking = false;

        //            Debug.Log("<color=green>TalentAgencyManager</color>: scout [<color=blue>" +
        //                scouts[i].name + "</color>] has finished his job");
        //        }
        //    }
        //}
    }
}
