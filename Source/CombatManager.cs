using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Prototypes.AI;

namespace Colosseum.Match {
    public class CombatManager : SingletonInstance<CombatManager> {
        /// <summary>
        /// Makes contestants plan their combat actions and exectute them
        /// </summary>
        /// <param name="team"> current team </param>
        public IEnumerator PlayTurn(Team team) {
            List<HumanoidContestant> contestants = new List<HumanoidContestant>(team.fieldContestants);
            // do not handle of dead guys
            contestants.RemoveAll(c => c.isDead);
            

            foreach (var contestant in contestants) {
                AIAgent agent = CreateConstestantAgent(contestant);
                Queue<AIAction> actionSequence = new Queue<AIAction>();
                List<AIAction> availibleActions = CreateContestantActions(contestant);
                bool result = AIPlanner.Plan(agent, availibleActions, actionSequence);
                if (!result) {
                    Debug.Log("failed to build plan for " + contestant.fighter.fighterName);
                    continue;
                }
                else {
                    //DebugPrettyPrintPlan(actionSequence, contestant.fighter.fighterName);
                }

                yield return agent.PerformActionSequence(actionSequence);
            }

        }

        /// <summary>
        /// Create contestant availible combat actions
        /// </summary>
        /// <param name="team"> current team </param>
        private List<AIAction> CreateContestantActions(HumanoidContestant contestant) {
            List<AIAction> actions = new List<AIAction>();

            AIAction baseAttackAction = new AIAction();
            baseAttackAction.name = "Base attack";
            baseAttackAction.action = PerformContestantBaseAttackAction(contestant);
            baseAttackAction.cost = 1;
            baseAttackAction.effects["hit target"] = true;

            AIAction performSkillAction = new AIAction();
            performSkillAction.name = "Perform skill";
            performSkillAction.cost = 0;
            performSkillAction.preconditions["has usable skills"] = true;
            performSkillAction.effects["hit target"] = true;
            performSkillAction.action = PerformContestantSkillAction(contestant);

            actions.Add(baseAttackAction);
            actions.Add(performSkillAction);

            return actions;
        }

        /// <summary>
        /// With this action contestant will attack with weapon
        /// </summary>
        private IEnumerator PerformContestantBaseAttackAction(HumanoidContestant contestant) {
            contestant.portrait.HighlightPortrait();
            List<ContestantBody> targets = FindContestantTargets(contestant, contestant.Weapon, null);

            if (targets.Count == 0) {
                //Debug.Log("<color=blue>" + contestant.fighter.fighterName + "</color> cannot perform action because he has no targets");
                // Action cannot be performed
                contestant.portrait.UnhighlightPortrait();
                yield break;
            }

            var animation = contestant.body.PlayWeaponAnimation();
            while (animation.IsPlaying) {
                ArenaGrid.Instance.ShowTargetZone(contestant, Color.green);

                if (animation.DequeueEvent("impact")) {
                    // Hit our targets
                    foreach (var target in targets) {
                        if (target == null || target.GetContestant().isDead)
                            continue; // skip if for some reason the target is dead or destroyed

                        //Debug.Log(contestant.fighter.fighterName + " damages " + t.GetFighter().fighterName + "with weapon");

                        HitData hit = new HitData();
                        hit.agressor = contestant.body;
                        hit.incomingDamage = contestant.Weapon.attackDamage;
                        hit.destination = target.GetHitLocation();
                        hit.hitTarget = target;
                        hit.projectile = null;
                        hit.skill = null;
                        hit.sourceWeapon = contestant.Weapon;

                        // Kinda send Hit event
                        target.OnHit(hit);
                    }

                }
                // Handle ranged weapons
                else if (animation.DequeueEvent("launch")) {
                    foreach (var target in targets) {
                        Projectile projectile = contestant.body.SpawnWeaponProjectile();

                        HitData projectileInstruction = new HitData();
                        projectileInstruction.agressor = contestant.body;
                        projectileInstruction.incomingDamage = contestant.Weapon.attackDamage;
                        projectileInstruction.destination = target.GetHitLocation();
                        projectileInstruction.hitTarget = target;
                        projectileInstruction.projectile = projectile;
                        projectileInstruction.skill = null;
                        projectileInstruction.sourceWeapon = contestant.Weapon;

                        projectile.Launch(projectileInstruction);
                    }

                }

                yield return null;
            }

            ArenaGrid.Instance.ResetGridCellsAppearance();
            //UI.UIMatchManager.Instance.RefreshContestantPortraits();
            contestant.portrait.UnhighlightPortrait();
        }

        /// <summary>
        /// With this action contestant will attack with skill
        /// </summary>
        private IEnumerator PerformContestantSkillAction(HumanoidContestant contestant) {
            List<Skill> skills = GetContestantUsableSkills(contestant);
            Skill randomSkill = skills[Random.Range(0, skills.Count)];

            contestant.portrait.HighlightPortrait();
            contestant.portrait.HighlightSkill(randomSkill, true);

            List<ContestantBody> targets = FindContestantTargets(contestant, contestant.Weapon, randomSkill);
            if (targets.Count == 0) {
                // Action cannot be performed
                contestant.portrait.HighlightSkill(randomSkill, false);
                contestant.portrait.UnhighlightPortrait();
                yield break;
            }

            contestant.ModifyActionPoints(-randomSkill.actionPointCost);
            randomSkill.ActivateCoolDown();
            contestant.portrait.Refresh();
            float incomingDamage = contestant.Weapon.attackDamage + Mathf.Abs(randomSkill.healthModifier);

            var animation = contestant.body.PlayWeaponAnimation();
            while (animation.IsPlaying) {
                ArenaGrid.Instance.ShowTargetZone(contestant, Color.yellow, randomSkill);

                if (animation.DequeueEvent("impact")) {
                    // Hit our targets
                    foreach (var target in targets) {
                        if (target == null || target.GetContestant().isDead)
                            continue; // skip if for some reason the target is dead or destroyed

                        HitData hit = new HitData();
                        hit.agressor = contestant.body;
                        hit.incomingDamage = incomingDamage;
                        hit.destination = target.GetHitLocation();
                        hit.hitTarget = target;
                        hit.projectile = null;
                        hit.skill = randomSkill;
                        hit.sourceWeapon = contestant.Weapon;

                        // Kinda send Hit event
                        target.OnHit(hit);
                    }

                }
                // Handle ranged weapons
                else if (animation.DequeueEvent("launch")) {
                    foreach (var target in targets) {

                        Projectile projectile = contestant.body.SpawnWeaponProjectile();

                        HitData projectileInstruction = new HitData();
                        projectileInstruction.agressor = contestant.body;
                        projectileInstruction.incomingDamage = incomingDamage;
                        projectileInstruction.destination = target.GetHitLocation();
                        projectileInstruction.hitTarget = target;
                        projectileInstruction.projectile = projectile;
                        projectileInstruction.skill = randomSkill;
                        projectileInstruction.sourceWeapon = contestant.Weapon;

                        projectile.Launch(projectileInstruction);
                    }

                }

                yield return null;
            }

            ArenaGrid.Instance.ResetGridCellsAppearance();
            //UI.UIMatchManager.Instance.RefreshContestantPortraits();
            contestant.portrait.HighlightSkill(randomSkill, false);
            contestant.portrait.UnhighlightPortrait();
        }

        /// <summary>
        /// Find all contestant usable skills
        /// </summary>
        private List<Skill> GetContestantUsableSkills(HumanoidContestant contestant) {
            List<Skill> usableSkills = contestant.GetStanceUsableSkills();
            usableSkills.RemoveAll(s => s.actionPointCost > contestant.ActionPoints || s.IsCoolingDown());

            return usableSkills;
        }

        /// <summary>
        /// Find all contestant targets
        /// </summary>
        private List<ContestantBody> FindContestantTargets(HumanoidContestant contestant, Weapon weapon, Skill skill) {
            return ArenaGrid.Instance.GetContestantTargetZoneTargets(contestant, weapon, skill);
        }

        /// <summary>
        /// Creates contestant agent that will perform planned actions
        /// </summary>
        private AIAgent CreateConstestantAgent(HumanoidContestant contestant) {
            AIAgent agent = new AIAgent();
            agent.worldState["has usable skills"] = GetContestantUsableSkills(contestant).Count > 0;
            agent.goalState["hit target"] = true;
            return agent;
        }

        private void DebugPrettyPrintPlan(Queue<AIAction> plan, string actorName) {
            string planMsg = "<color=blue>" + actorName + "</color> plan built: ";
            foreach (var a in plan) {
                planMsg += a.name + "<color=blue> -> </color>";
            }
            planMsg += " Hit Target";
            Debug.Log(planMsg);
        }
    }

}
