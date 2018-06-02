using System.Collections.Generic;
using UnityEngine;
using Colosseum.Management.UI;

namespace Colosseum.Management {
    // FUTURE: Maybe integrate effect system in healing
	public class ClinicManager : SingletonInstance<ClinicManager> {
        public int level = 1; // level of the building
        public int patientCapacity = 4; // the max patiens that building can hold
        public int maxPatientCapacity = 9; // max slot amount
        public float healthRecoveryAmount = 40.0F; // how much health will be recovered each turn

        [System.NonSerialized] public HumanoidFighter[] patients = new HumanoidFighter[4];
        
        // Tells How much health will be recovered based on patients amount
        public float GetTurnHealthGain() {
            float healthPerTurn = Mathf.Ceil((healthRecoveryAmount / patients.Length));
            if (healthPerTurn == Mathf.Infinity)
                return 0;

            return healthPerTurn;
        }

        // Add a specific patient to the clinic
        public bool AddPatient(HumanoidFighter fighter, int podIndex) {
            //if (fighter.currentHealth > 20.0f)
            //fighter.currentHealth -= 10.0F;
            Debug.Log(patients.Length);
            Debug.Log(podIndex);

            if (patients[podIndex] != null || 
                fighter.currentHealth == fighter.MaxHealth ||
                fighter.activity != FighterActivity.None)
                return false;
            
            // Remove from other locations.
            if(GymManager.Instance.IsBusyTraining(fighter))
                GymManager.Instance.RemoveStudent(fighter);

            patients[podIndex] = fighter;
            fighter.activity = FighterActivity.Resting;
            UIManagementManager.Instance.RefreshInventoryView();

            return true;
		}
        
        // Remove the patient from the clinic
        public bool RemovePatient(HumanoidFighter fighter) {
            int index = System.Array.IndexOf(patients, fighter);
            if (index == -1)
                return false;

            patients[index].activity = FighterActivity.None;
            patients[index] = null;
            UIManagementManager.Instance.RefreshInventoryView();
            return true;
        }

        // Is given fighter a patient of the clinic
	    public bool IsPatient(HumanoidFighter fighter) {
	        return System.Array.Exists(patients, f => f == fighter);
	    }
        
        // Heals the gladiators (by turn)
        public void HealPatients() {
            for (int i = patients.Length - 1; i >= 0; i--) {
                if (patients[i] == null)
                    continue;

                patients[i].currentHealth += GetTurnHealthGain();

                if (patients[i].currentHealth > patients[i].MaxHealth) {
                    patients[i].currentHealth = patients[i].MaxHealth;
                    
                    string resultText = patients[i].fighterName + " has been fully healed";
                    
                    RemovePatient(patients[i]);

                    AdvisorManager.Instance.dayEventResultList.Add(resultText);
                }

                UIManagementManager.Instance.RefreshInventoryView();
            }
        }

        // Increase capacity of the clinic
        public void IncreasePatientCapacity(int count) {

            // TODO: Trigger additional requirement.
            patientCapacity += count;
        }

        // Let fighter consume the given drug
        public void ConsumeDrug(Drug drug, HumanoidFighter fighter) {
            drug.Consume(fighter);
            InventoryManager.Instance.ownDrugs.Remove(drug);
            UIManagementManager.Instance.RefreshInventoryView();
        }

        //protected void Awake() {
        //    // REMOVE: Remove when buying drugs is implemented
        //    for (int i = 0; i < 5; i++) {
        //        Drug drug = new Drug();
        //        //drug.image = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Match/Audience Modifier Icons/ca_world_style-[audience-motivator_feromones]_2017-05-15.png");
        //        //Debug.Log(drug.image);
        //        drug.drugName = "drug_" + drug.GetHashCode();
        //        drug.effect = new FighterEffect();
        //        drug.effect.baseHealthGain = Random.Range(10, 30);
        //        drug.effect.moraleLoss = Random.Range(1, 3);
        //        drug.bonusEffect = new FighterEffect();
        //        drug.bonusEffect.baseStrengthGain = Random.Range(10, 20);
        //        drug.flavorText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit." +
        //            "Etiam gravida purus a rhoncus pharetra. Morbi faucibus lobortis enim, eget " +
        //            "dignissim tellus interdum quis. Suspendisse elementum cursus elementum. " +
        //            "In pharetra, arcu id pellentesque ultricies, nisi orci laoreet ante, sed varius nisl " +
        //            "lorem sit amet odio. Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
        //            "Nunc purus urna, laoreet id tempus eu, scelerisque porta elit.";
        //        drug.functionalDescription = "Instant effect: \n" + ColosseumUtils.CreateEffectDescription(drug.effect) + "\n";
        //        drug.functionalDescription += "Bonus effect: \n" + ColosseumUtils.CreateEffectDescription(drug.bonusEffect) + "\n";

        //        drug.isMatchPhaseUsable = Random.Range(0, 1) == 1;
        //        drug.price = Random.Range(50, 500);
        //        drug.isMatchPhaseUsable = false;
        //        drug.price = Random.Range(100, 500);
        //        InventoryManager.Instance.ownDrugs.Add(drug);
        //    }
        //    //
        //}
        
        // Validate the state of all patients of the clinic
        public void ValidateOccupiedFighters() {
            for (int i = patients.Length - 1; i >= 0; i--) {
                if (!InventoryManager.Instance.ownHumanoids.Contains(patients[i])) {
                    RemovePatient(patients[i]);
                    //patients[i].activity = FighterActivity.None;
                    //patients.RemoveAt(i);
                }
            }

            UIManagementManager.Instance.RefreshInventoryView();
        }


        // DEBUG
        /*public void DebugDecreaseInventoryFightersHealth() {
            List<HumanoidFighter> fighters = InventoryManager.Instance.ownHumanoids;
            for (int i = 0; i < fighters.Count; i++) {
                fighters[i].currentHealth = fighters[i].MaxHealth - 5.0F;
            }

            Colosseum.UI.InventoryView.Instance.Refresh();
        }*/
    }

}
