using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.Management {
    public class Sector {
        public string name; // the name of the sector
        public AudienceSpecies species; // the species populating the sector
        public PopulationDensity populationDensity; // the species population density of the sector
        public string history; // flavor text of the sector
        public bool isLocked; // is sector locked? (Not yet unlocked by the player/game)

        // Unlock specific events when sector is unlocked
        public void UnlockEvents() {
            // TODO: Handle additional events here
            throw new System.NotImplementedException();
        }
    }
}
