using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Colosseum.Management.UI;

namespace Colosseum.Management {
	public class GymManager : SingletonInstance<GymManager> {
        public int roomCapacity = 5;
        public int maxTrainingRoomAmount;
        public int hireCost = 500;
        public int restockCost = 100;

        // applicant, prospect
        public List<Instructor> applicants = new List<Instructor>(); 
        public List<Instructor> instructors = new List<Instructor>();
        public List<TrainingRoom> rooms = new List<TrainingRoom>();

        //// Wordt straks setup, moet eenmal gecald worden door het hele spel
        //protected void Awake() {
        //    //AddTrainingRoom();
        //    CreateDemoRoom();
        //}

        public void ExecuteTrain() {
            bool hasFighterDataChanged = false;

            for (int i = 0; i < rooms.Count; i++) {
                if (rooms[i].instructor == null)
                    continue;

                for (int j = rooms[i].students.Count - 1; j >= 0; j--) {
                    if (!rooms[i].instructor.Train(rooms[i].students[j])) {
                        string resultedText = rooms[i].students[j].fighter.fighterName + " learned skill \'" + rooms[i].students[j].fighter.skills[rooms[i].students[j].fighter.skills.Count - 1].skillName + "\'.";
                        //+ " from instructor and has left the training room";
                        
                        Debug.Log(resultedText);
                        
                        AdvisorManager.Instance.dayEventResultList.Add(resultedText);

                        rooms[i].Remove(rooms[i].students[j]);
                        hasFighterDataChanged = true;
                    }
                }
}
            if (hasFighterDataChanged)
                UIManagementManager.Instance.RefreshInventoryView();
        }

	    public bool IsBusyTraining(HumanoidFighter fighter) {
	        foreach(var room in rooms)
	            if(room.HasStudent(fighter))
	                return true;

	        return false;
	    }

        //public bool HireInstructor(Instructor instructor) {
        //    if (Inventory.Instance.Moolah < hireCost || 
        //        instructors.Count == rooms.Count) {
        //        return false;
        //    }

        //    // For now we just adding it to the list
        //    instructors.Add(instructor);
        //    applicants.Remove(instructor);
        //    Inventory.Instance.ModifyMoolah(-hireCost);

        //    return true;
        //}

        //public void FireInstructor(Instructor instructor) {
        //    instructors.Remove(instructor);
        //}

        public void CreateRoom(Instructor instructor, int availiblePlaceCount, int capacity) {
            TrainingRoom room = new TrainingRoom();
            room.instructor = instructor;
            room.availiblePlaceCount = availiblePlaceCount;
            room.capacity = capacity;
            rooms.Add(room);
        }

        public Student AddStudent(TrainingRoom room, HumanoidFighter fighter) {
            if (room.HasStudent(fighter))
                return null;
            
            // Remove from other locations.
            if(ClinicManager.Instance.IsPatient(fighter))
                ClinicManager.Instance.RemovePatient(fighter);
            
            
            Student student = new Student(fighter);
            room.Populate(student);
            return student;
        }

        public void RemoveStudent(TrainingRoom room, Student student) {
            room.Remove(student);
        }
	    
	    public void RemoveStudent(Fighter student) {
	        foreach(var room in rooms) {
	            foreach(var roomStudent in room.students) {
	                if(roomStudent.fighter == student) {
	                    room.Remove(roomStudent);
	                    return;
	                }
	            }
	        }
	    }

        public void ValidateOccupiedFighters() {
            for (int i = rooms.Count - 1; i >= 0; i--) {
                for (int j = 0; j < rooms[i].students.Count; j++) {
                    if (!InventoryManager.Instance.ownHumanoids.Contains(rooms[i].students[i].fighter))
                        rooms[i].Remove(rooms[i].students[i]);
                }
            }

            UIManagementManager.Instance.RefreshInventoryView();
        }

        // TODO: remove junk
        //public bool RestockApplicants(bool isPriced) {
        //    Result result;
        //    result.value = true;
        //    result.message = null;

        //    if (isPriced) {
        //        result.value = Inventory.Instance.Money >= restockCost;

        //        if (!result.value) {
        //            result.message = "Not enough money";
        //            return result;
        //        }

        //        Inventory.Instance.ModifyMoney(-restockCost);
        //    }

        //    List<Instructor> newApplicants = new List<Instructor>(GameManager.Instance.instructors);
        //    newApplicants.RemoveAll(instructor => applicants.Contains(instructor));

        //    applicants.Clear();

        //    for (int i = 0; i < 3; i++) {
        //        int index = Random.Range(0, newApplicants.Count);
        //        applicants.Add(newApplicants[index]);
        //        newApplicants.RemoveAt(index);
        //    }

        //    restockCost *= restockCost;
        //    return result;
        //}

        /* Legacy, Tests...
        protected void Awake() {
            instance = this;

            //List<Skill> skills = GameManager.Instance.skills;

            //// TODO: This is temp, til we load them from db
            //instructors = new List<Instructor>();
            //for (int i = 0; i < 3; i++) {
            //    Instructor trainer = new Instructor();

            //    for (int k = 0; k < 3; k++) {
            //        trainer.skills.Add(skills[k]);
            //    }

            //    // Randomly repeat availible skills to increase chance
            //    int skillCount = trainer.skills.Count;
            //    for (int k = 0; k < skillCount; k++) {
            //        if (Random.value < 0.5F) {
            //            trainer.skills.Add(trainer.skills[k]);
            //        }   
            //    }

            //    trainer.skillGainXPTreshold = 100;
            //    trainer.trainingRate = 0.5F;
            //    trainer.studentCapacity = Random.Range(1, 4);
            //    trainer.salary = Random.Range(100, 150);
            //    instructors.Add(trainer);
            //}

            //instructors[0].image = Resources.Load<Sprite>("Sprites/trainer1");
            //instructors[0].instructorName = "Nuke Dukem";
            //instructors[1].image = Resources.Load<Sprite>("Sprites/trainer2");
            //instructors[1].instructorName = "Mr. Giggles";
            //instructors[2].image = Resources.Load<Sprite>("Sprites/trainer3");
            //instructors[2].instructorName = "Screecher";
        }
        */
    }

}
