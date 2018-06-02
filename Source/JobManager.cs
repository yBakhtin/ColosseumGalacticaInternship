using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Management.UI;

namespace Colosseum.Management {
    public class JobManager : SingletonInstance<JobManager> {
        public List<Job> jobOffers = new List<Job>();
        public List<Job> persistentJobOffers = new List<Job>();
        public List<Job> activeJobs = new List<Job>();
        public int maxJobOfferAmount = 5;

        // FIXME: Execution order!!!
        protected void Start() {
            // TEMP!!!!:
            // Generate galaxy jobs
            for (int i = 0; i < 10; i++) {
                Job job = new Job();
                job.category = JobCategory.Regular;
                job.description = "etc.";
                job.duration = Random.Range(1, 9);
                job.expiration = Random.Range(1, 9);
                job.isCancelable = false;
                //job.isPersistent = false;
                job.jobName = "job_" + job.GetHashCode().ToString();

                job.penalty = new List<Effect>();
                job.penalty.Add(new Effect());
                job.penalty[0].isHidden = false;
                //job.penalty[0].description = "etc.";
                job.penalty[0].creditsLoss = Random.Range(0, 1000);
                job.penalty[0].reputationLoss = Random.Range(0, 50);
                job.penalty[0].coachingInfluenceLoss = Random.Range(0, 10);
                //job.penalty[0].parent = job;

                job.requiredEmployeeAmount = 5;
                job.requirement = new JobRequirement();
                job.requirement.minStrength = Random.Range(1, 3);

                job.reward = new List<Effect>();
                job.reward.Add(new Effect());
                job.reward[0].creditsGain = Random.Range(0, 177);
               // job.reward[0].parent = job;


                job.turnEffects = new List<Effect>();
                FighterEffect turnEffect = new FighterEffect();
                // turnEffect.creditsLoss = Random.Range(10, 100);
                //turnEffect.lostGladiatorAmount = 1;
                turnEffect.coachingInfluenceGain = 10;
                turnEffect.coachingInfluenceLoss = 2;
                turnEffect.creditsGain = 12;
                //turnEffect.lostAudienceManipulatorAmount = 2;
                //turnEffect.lostDrugAmount = 2;
                //turnEffect.lostShowEnhancerAmount = 2;
                //turnEffect.lostWeaponAmount = 2;
                //turnEffect.reputationGain = 2;
                //turnEffect.reputationLoss = 3;
                turnEffect.moraleLoss = Random.Range(i, 10);
                //turnEffect.moraleGain = Random.Range(i, 20);
                //turnEffect.baseStrengthGain = Random.Range(i, 20);
                //turnEffect.baseStrengthLoss = Random.Range(i, 20);
                //turnEffect.lostSkillAmount = 2;
                //turnEffect.lostTraitAmount = 2;

                job.turnEffects.Add(turnEffect);
                //GameManager.Instance.content.jobs.Add(job);
            }

            

            //// TEMP!!!!:
            //Management.Job autograph = new Management.Job();
            //autograph.category = Management.JobCategory.Permanent;
            //autograph.description = "Autograph";
            //autograph.duration = -1;
            //autograph.expiration = 0;
            //autograph.isCancelable = false;
            //autograph.isPersistent = true;
            //autograph.jobName = "Autograph signing";
            ////autograph.penalty = new List<JobEffect>();
            ////autograph.penalty.reputationLoss = 10;
            //autograph.requiredEmployeeAmount = 1;
            //autograph.requirement = new JobRequirement();
            //autograph.requirement.minPopularity = 10;

            //autograph.reward = new List<JobEffect>();
            //autograph.reward.Add(new JobEffect());
            //autograph.reward[0].reputationGain = 10;
            //autograph.reward[0].parent = autograph;


            //autograph.turnEffects = new List<Management.JobEffect>();
            //autograph.turnEffects.Add(new FighterJobEffect());
            //((Management.FighterJobEffect)autograph.turnEffects[0]).moraleLoss = 20;

            //autograph.requirement.additionalRequirements.Add(fighter => fighter.morale > 0);

            //jobs.Add(autograph);


            //persistentJobOffers = GetPersistenJobs();
            //for (int i = 0; i < jobs.Count; i++) {
            //    if (jobs[i].isPersistent) {
            //        persistentJobOffers.Add(jobs[i]);
            //    }
            //}

            //Debug.Log(Inventory.Instance.ownedHumanoids[0].popularity);
            // TEMP:!!!
        }

        public void StartJobs() {
            // Add usual job offers
            for (int i = jobOffers.Count - 1; i >= 0; i--) {
                if (jobOffers[i].requiredEmployeeAmount == jobOffers[i].employees.Count) {
                    activeJobs.Add(jobOffers[i]);
                    jobOffers.RemoveAt(i);
                }
            }

            // Add persistent job offers
            //for (int i = persistentJobOffers.Count - 1; i >= 0; i--) {
            //    if (persistentJobOffers[i].requiredEmployeeAmount == persistentJobOffers[i].employees.Count) {
            //        activeJobs.Add(persistentJobOffers[i]);
            //        persistentJobOffers.Remove(persistentJobOffers[i]);
            //    }
            //}

            //Debug.Log("Active job count: "  +activeJobs.Count);
        }

        public void PerformJobs() {
            for ( int i = activeJobs.Count - 1; i >= 0; i-- ) {
                Job job = activeJobs[i];

                if (job.employees.Count != job.requiredEmployeeAmount)
                    continue;

                if ( !job.Perform() ) {
                    Debug.Log("Job is finished");
                    activeJobs.Remove(job);
                    UIManagementManager.Instance.RefreshInventoryView();
                    ReplenishJobOffers();
                    //// TODO: Reimplement
                    //List<Job> uniqueOffers = GetRandomJobOffer();
                    //// suniqueOffers.Count == 0 should basically never happen, but in case it happens
                    //// how would we handle this?
                    //if (uniqueOffers.Count > 0)
                    //    jobOffers.Add(uniqueOffers[Random.Range(0, uniqueOffers.Count)]);
                }
            }
        }

        #region Job offer management

        public void ExpireJobOffers() {
            for (int i = jobOffers.Count - 1; i >= 0; i--) {
                if (jobOffers[i].progressAmount == 0)
                    jobOffers[i].expiration--;

                if (jobOffers[i].expiration == 0) {
                    //Debug.Log("Job offer: " + jobOffers[i].jobName + " expired!");

                    for (int j = jobOffers[i].employees.Count - 1; j >= 0; j--)
                        CancelEmploye(jobOffers[i], jobOffers[i].employees[j]);

                    jobOffers.RemoveAt(i);
                }
            }
        }

        public void ReplenishJobOffers() {
            int newJobOfferAmount = maxJobOfferAmount - (jobOffers.Count + activeJobs.Count);
            //Debug.Log("newJobOfferCount: " + newJobOfferAmount);
            for (int i = 0; i < newJobOfferAmount; i++) {
                Job jobOffer = GetRandomJobOffer();

                if (jobOffer != null)
                    jobOffers.Add(jobOffer);
            }
        }

        // TODO: Dublicates kommen in de offers. Moet fixen!!!
        private Job GetRandomJobOffer() {
            List<Job> offers = GameManager.Instance.content.jobs.FindAll(
                job =>
                !job.isCancelable &&
                !jobOffers.Exists(offer => offer.jobName == job.jobName) &&
                !activeJobs.Exists(activeJob => activeJob.jobName == job.jobName)
                );

            if (offers.Count == 0)
                return null;

            return offers[Random.Range(0, offers.Count)].Clone();
        }

        // Used by the ManagementManager SetUp to prepare the JobManager for use;
        // TODO: Could potentially get renamed to SetUp.
        public void CollectPersistentJobs(List<Job> allGameJobs) {
            for (int i = 0; i < allGameJobs.Count; i++) {
                if (!allGameJobs[i].isCancelable)
                    jobOffers.Add(allGameJobs[i]);
            }
        }

        #endregion

        public bool AssignEmploye(Job job, HumanoidFighter fighter) {
            if (job.employees.Count == job.requiredEmployeeAmount ||
                fighter.activity != FighterActivity.None ||
                !job.requirement.Validate(fighter) ||
                job.employees.Contains(fighter))
                return false;

            job.employees.Add(fighter);
            fighter.activity = FighterActivity.PerformingJob;
            UIManagementManager.Instance.RefreshInventoryView();

            if (job.reward != null)
                for (int i = 0; i < job.reward.Count; i++) {
                    FighterEffect rewardEffect = job.reward[i] as FighterEffect;
                    if (rewardEffect != null) {
                        rewardEffect.fighterTargets.Add(fighter);
                    }
                }

            if (job.penalty != null)
                for (int i = 0; i < job.penalty.Count; i++) {
                    FighterEffect penaltyEffect = job.penalty[i] as FighterEffect;
                    if (penaltyEffect != null) {
                        penaltyEffect.fighterTargets.Add(fighter);
                    }
                }

            if (job.turnEffects != null)
                for (int i = 0; i < job.turnEffects.Count; i++) {
                    FighterEffect turnEffect = job.turnEffects[i] as FighterEffect;
                    if (turnEffect != null) {
                        turnEffect.fighterTargets.Add(fighter);
                    }
                }

            return true;
        }

        public bool CancelEmploye(Job job, HumanoidFighter fighter) {
            if (job.employees.Remove(fighter)) {
                fighter.activity = FighterActivity.None;
                UIManagementManager.Instance.RefreshInventoryView();
                return true;
            }

            return false;
        }

        public void ValidateOccupiedFighters() {
            for (int i = 0; i < jobOffers.Count; i++) {
                for (int j = jobOffers[i].employees.Count - 1; j >= 0; j--) {
                    HumanoidFighter employee = jobOffers[i].employees[j];

                    if (!InventoryManager.Instance.ownHumanoids.Contains(employee)) {
                        jobOffers[i].employees[j].activity = FighterActivity.None;
                        jobOffers[i].employees.RemoveAt(j);
                    }
                }
            }

            for (int i = activeJobs.Count - 1; i >= 0; i--) {
                bool isAnyEmployeeInvalid = false;

                for (int j = activeJobs[i].employees.Count - 1; j >= 0; j--) {
                    HumanoidFighter employee = activeJobs[i].employees[j];

                    if (!InventoryManager.Instance.ownHumanoids.Contains(employee)) {
                        isAnyEmployeeInvalid = true;
                        break;
                    }
                }

                if (isAnyEmployeeInvalid) {
                    activeJobs[i].Done();
                    activeJobs.RemoveAt(i);
                }
            }

            UIManagementManager.Instance.RefreshInventoryView();
        }


        // Old...
        //public void CancelJob(Job job) {
        //    for (int i = job.employees.Count - 1; i >= 0; i--)
        //        CancelEmploye(job, job.employees[i]);

        //    activeJobs.Remove(job);
        //    UIManagementManager.Instance.RefreshFighterPool();
        //}

        //public void ReplenishJobOffers() {
        //    List<Job> jobs = new List<Job>(GameManager.Instance.jobs);
        //    List<Job> galaxyJobs = new List<Job>();
        //    for (int i = 0; i < jobs.Count; i++) {
        //        if (!jobs[i].isPersistent)
        //            galaxyJobs.Add(jobs[i]);
        //    }

        //    int freeJobPlaceAmount = maxJobOfferAmount - jobOffers.Count;
        //    // Needs testing!!!
        //    for (int i = 0; i < freeJobPlaceAmount; i++) {
        //        int offerIndex = Random.Range(i, galaxyJobs.Count); // safe?
        //        Job jobOffer = galaxyJobs[offerIndex];
        //        jobOffers.Add(jobOffer);
        //        Debug.Log("new job offer: " + jobOffer.jobName);
        //        // Swap/shuffle array elements
        //        GameManager.Instance.jobs[i] = jobOffer;
        //        GameManager.Instance.jobs[offerIndex] = jobs[i];
        //    }
        //}

        // Get all the persistent jobs and return them
        //private List<Job> GetPersistenJobs() {
        //    List<Job> returnedJobs = new List<Job>();

        //    List<Job> jobs = GameManager.Instance.jobs;
        //    for (int i = 0; i < jobs.Count; i++) {
        //        if (jobs[i].isPersistent) {
        //            returnedJobs.Add(jobs[i]);
        //        }
        //    }

        //    return returnedJobs;
        //}

        //bool StartJob(Job job) {
        //    if (job.IsPerforming || !job.HasFullPersonal)
        //        return false;

        //    jobProspects.Remove(job);
        //    activeJobs.Add(job);

        //    return true;
        //}

        //public bool CancelJob(Job job) {
        //    job.Cancel();
        //    return activeJobs.Remove(job);
        //}
    }
}
