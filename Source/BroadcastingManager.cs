using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.Management {
    // TODO: Koppelen naar db
    public class BroadcastingManager : SingletonInstance<BroadcastingManager> {
        public float goldMultiplier = 1000;
        public float populationDensityLow = 0.5f;
        public float populationDensityMid = 0.75f;
        public float populationDensityHigh = 1f;
        public int unlockedSectorAmount;
        public int maxSectorAmount = 9; // element count must be the same as sector count!!!
        public Sector[] sectors;

        public string[] speciesNames = { "Klingon", "Romulan", "Human" };

        public string[] sectorNames = { "Acreania", "Nachezuno", "Uplides", "Rebrorth",
                                         "Kaylia", "Daystea", "Flayulia", "Prorocury",
                                         "Grinda UE"
        };

        public string[] sectorHistories = {
            "Centuries of wars between the Sector A and Sector B",
            "A peaceful sector with lots of resources and friendly people",
            "Very unfriendly sector. People here do not like the newcommers and strangers",
            "Centuries of wars between the Sector A and Sector C",
            "Centuries of wars between the Sector C and Sector B",
            "Centuries of wars between the Sector B and Sector C",
            "Poor sector with little resources",
            "Crowded sector with lots of people and thereby lacks of resources",
            "Abandoned sector with few residents living in, but has lots of recources"
        };

        // The cost that is used for unlocking the sector
        public float UnlockCost { get { return unlockedSectorAmount * goldMultiplier + 1000; } }

        // Verify that any requirements are met before purchasing a sector
        public bool ValidateUnlockRequirements(Sector sector, Sector parentSector) {
            bool result = false;
            
            result = InventoryManager.Instance.Moolah >= UnlockCost;
            if (parentSector != null)
                result &= !parentSector.isLocked;
            return result;
        }

        // Unlock the given sector
        public bool UnlockSector(Sector sector) {
            unlockedSectorAmount++;
            InventoryManager.Instance.ModifyMoolah(-UnlockCost);
            sector.isLocked = false;
            return true;
        }

        // Get population density of all sectors
        public float GetAllPopulationDensity() {
            float populationDensity = 0;

            for (int i = 0; i < sectors.Length; i++) {
                if (sectors[i].isLocked)
                    continue;

                if (sectors[i].populationDensity == PopulationDensity.Low)
                    populationDensity += populationDensityLow;
                if (sectors[i].populationDensity == PopulationDensity.Mid)
                    populationDensity += populationDensityMid;
                if (sectors[i].populationDensity == PopulationDensity.High)
                    populationDensity += populationDensityHigh;
            }

            return populationDensity;
        }
        
        // Generates the sectors
        public void GenerateSectors() {
            sectors = new Sector[maxSectorAmount]; // TODO: Can be moved to global ctor initialization
            for ( int i = 0; i < maxSectorAmount; i++ ) {
                Sector sector = sectors[i];
                sector = new Sector();

                int sectorNameIndex = Random.Range(i, sectorNames.Length);
                sector.name = sectorNames[sectorNameIndex];
                string sectorNameBuffer = sectorNames[i];
                sectorNames[i] = sectorNames[sectorNameIndex];
                sectorNames[sectorNameIndex] = sectorNameBuffer;

                sector.species = new AudienceSpecies();
                int nameIndex = Random.Range(0, speciesNames.Length); // if unique 0 = i
                sector.species.name = speciesNames[nameIndex];
                // Uncomment if the case is thrown that species should have a unique name
                //string speciesNameBuffer = speciesNames[i];
                //speciesNames[i] = speciesNames[nameIndex];
                //speciesNames[nameIndex] = speciesNameBuffer;

                sector.populationDensity = (PopulationDensity)Random.Range(0, 2);

                int sectorHistoryIndex = Random.Range(0, sectorHistories.Length);
                sector.history = sectorHistories[sectorHistoryIndex];
                string sectorHistoryBuffer = sectorHistories[i];
                sectorHistories[i] = sectorHistories[sectorHistoryIndex];
                sectorHistories[sectorHistoryIndex] = sectorHistoryBuffer;

                sector.isLocked = true;
                sectors[i] = sector;
            }
        }
    }
}
