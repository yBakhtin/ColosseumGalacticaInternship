using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.Match {
    // Temp helper for preset handling
    [System.Serializable]
    public class ArenaGridPreset {
        public Vector2 scale;
        public Sprite perspectiveBackground;
        public Vector2 topLeft;
        public Vector2 bottomLeft;
        public Vector2 topRight;
        public Vector2 bottomRight;
        public float depth;
    }

    public class ArenaGrid : SingletonInstance<ArenaGrid> {
        public Vector3 topLeft;
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public Vector3 bottomRight;
        public float leftScaleMultiplier = 1.0f;    // Scale is multiplied by this on the left edge.
        public float rightScaleMultiplier = 1.0f;	// And on the right edge.
        public int rows = 4; // horizontal length of the grid 
        public int columns = 2; // vertical length of the grid
        public ContestantBody[,] actorGrid = new ContestantBody[4, 2]; // 4 rows and 2 columns
        public GridMeshBuilder grid;
        //public ArenaGridCell[,] cells;

        public float bottomZDepth = -2.0f;
        public float topZDepth = 0;

        // New arrays for contestant spots
        public Vector2Int[] leftTeamSpotArray;
        public Vector2Int[] rightTeamSpotArray;

        // Preset handling
        public ArenaGridPreset[] presets;
        private ArenaGridPreset currentPreset;
        public SpriteRenderer arenaBackgroundTemp;

        public int[,] leftTeamSpots = new int[4, 2] {
            {0,0}, 
            {0,1}, 
            {0,2}, 
            {0,3} // 4 rows and 2 columns (row, column)
        };

        public int[,] rightTeamSpots = new int[4, 2] {
            {1,0}, 
            {1,1}, 
            {1,2}, 
            {1,3} // 4 rows and 2 columns
        };

        /// <summary>
        /// Returns the position of a spot as a Vector3.
        /// </summary>
        /// <param name="teamNumber">The teamnumber</param>
        /// <param name="spotNumber">The spotnumber of a team</param>
        /// <returns></returns>
        public Vector3 GetSpotPosition(int teamNumber, int spotNumber) {
            if (teamNumber > 2)
                return Vector3.zero;

            Vector2 gridPosition = GetSpotGridPositionNew(teamNumber, spotNumber);

            // FIXME: This was causing the wrong position/scale of constestants at zero spot coordinate
            //if (gridPosition == Vector2.zero)
            //    return Vector3.zero;

            //Debug.Log((int)gridPosition.x + " " + (int)gridPosition.y);
            return GetPosition((int)gridPosition.y, (int)gridPosition.x);
        }

        public Vector3 GetSpotPosition(HumanoidContestant contestant) {
            return GetSpotPosition(contestant.teamNumber, contestant.spotNumber);
        }

       // Spot number is row, 
        public Vector2Int GetSpotGridPosition(int teamNumber, int spotNumber) {
            if (teamNumber == 1)
                return new Vector2Int(leftTeamSpots[spotNumber - 1, 0], leftTeamSpots[spotNumber - 1, 1]); 
            else if (teamNumber == 2)
                return new Vector2Int(rightTeamSpots[spotNumber - 1, 0], rightTeamSpots[spotNumber - 1, 1]);
            else {
                Debug.LogError("invalid teamNumber");
                return Vector2Int.zero;
            }
        }

        // TODO: Will need to rename this later.
        /// <summary>
        /// Get spot grid position of contestant given the team number and spot number
        /// </summary>
        /// <param name="teamNumber"></param>
        /// <param name="spotNumber"></param>
        public Vector2Int GetSpotGridPositionNew(int teamNumber, int spotNumber) {
            if (teamNumber == 1)
                return leftTeamSpotArray[spotNumber - 1];
            else if (teamNumber == 2)
                return rightTeamSpotArray[spotNumber - 1];
            else {
                Debug.LogError("invalid teamNumber");
                return Vector2Int.zero;
            }
        }

        public Vector2Int GetSpotGridPosition(HumanoidContestant contestant) {
            return GetSpotGridPosition(contestant.teamNumber, contestant.spotNumber);
        }

        public Vector3 GetPosition(int gridX, int gridY) {
            return grid.GetCell(gridX, gridY).Position;

            //float topLength = Vector3.Distance(topLeft, topRight); // length of top left
            //float topTilesWidth = topLength / columns;
            //float topColumnX = topLeft.x + (topTilesWidth * (gridX/* - 1*/)) + (topTilesWidth * 0.5f);

            //float bottomLength = Vector3.Distance(bottomLeft, bottomRight);
            //float bottomTilesWidth = bottomLength / columns;
            //float bottomColumnX = bottomLeft.x + (bottomTilesWidth * (gridX/* - 1*/)) + (bottomTilesWidth * 0.5f);

            //Vector3 topColumnPoint = new Vector3(topColumnX, topLeft.y, 0);
            //Vector3 bottomColumnPoint = new Vector3(bottomColumnX, bottomLeft.y, 0);

            //float verticalLength = Vector3.Distance(topColumnPoint, bottomColumnPoint);
            //float verticalTileHeight = verticalLength / rows;
            //float topGoalDistance = (verticalTileHeight * (gridY/* - 1*/)) + (verticalTileHeight * 0.5f);

            //Vector3 topBottomColumnDirection = Vector3.Normalize(bottomColumnPoint - topColumnPoint);

            //return topColumnPoint + (topBottomColumnDirection * topGoalDistance);
        }

        public float GetZValueForPosition(Vector2 position) {
            float top = Mathf.Max(topLeft.y, topRight.y);
            float bottom = Mathf.Min(bottomLeft.y, bottomRight.y);

            float alpha = (position.y - bottom) / (top - bottom);

            float arenaGridZ = transform.position.z;

            return Mathf.LerpUnclamped(arenaGridZ + bottomZDepth, arenaGridZ + topZDepth, alpha);
        }

        /// <summary>
        /// Get contestant modified flipped target zone combined wih weapon and skill
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        private int[,] GetContestantTargetZone(HumanoidContestant actor, Skill skill) {

            int[,] targetZone = actor.Weapon.targetZone;

            if (skill != null)
                targetZone = CombineTargetZones(actor.Weapon.targetZone, skill.targetZone);

            if (actor.teamNumber == 2)
                targetZone = targetZone.FlipHorizontal();

            return targetZone;
        }

        /// <summary>
        /// Returns all reachable targets for specific contestant based on his skill/weapon target zones
        /// </summary>
        public List<ContestantBody> GetContestantTargetZoneTargets(HumanoidContestant actor, Weapon weapon, Skill skill) {
            List<ContestantBody> resultTargets = new List<ContestantBody>();

            int[,] targetZone = GetContestantTargetZone(actor, skill);
            Vector2Int gridSearchPosition = GetSpotGridPositionNew(actor.teamNumber, actor.spotNumber); // x = row, y = column
            Vector2Int actorTargetZonePosition = GetActorTargetZoneLocalPosition(targetZone);
            Vector2Int searchRange = new Vector2Int(targetZone.GetLength(0), targetZone.GetLength(1));

            //if (actor.Fighter.fighterName == "Fronk Borin") {
            //    Debug.Log("actorGridPosition: " + gridSearchPosition);
            //    Debug.Log("actorTargetZoneLocation: " + actorTargetZonePosition);
            //    Debug.Log("searchRange: " + searchRange);

            //    Debug.Log("Fronk target zone");
            //    for (int row = 0; row < targetZone.GetLength(0); row++) { 
            //        string columnStr = string.Empty;

            //        for (int column = 0; column < targetZone.GetLength(1); column++) {
            //            columnStr += targetZone[row, column] + " ";
            //        }

            //        Debug.Log("row [" + row + "]: " + columnStr);
            //    }
            //}

            Vector2Int anchoredGridSearchPosition = gridSearchPosition - actorTargetZonePosition; // top left anchor
            var targets = GetArenaGridContestants(anchoredGridSearchPosition, searchRange);

            for (int x = 0; x < targetZone.GetLength(0); x++)// rows
                for (int y = 0; y < targetZone.GetLength(1); y++) {// columns
                    int targetZoneValue = targetZone[x, y];

                    if (targets[x, y] == null) // Skip nulls
                        continue;

                    if (targetZoneValue == 1) // Skip player
                        continue;

                    //  Pick only those of the opposite team
                    if (targetZoneValue == 2 && targets[x,y].GetContestant().teamNumber != actor.teamNumber && !targets[x,y].GetContestant().isDead) {
                        resultTargets.Add(targets[x, y]);
                    }
                }

            return resultTargets;
        }

        /// <summary>
        /// Combine multiple target zones in one.
        /// </summary>
        public int[,] CombineTargetZones(params int[][,] targetZones) {
            if (targetZones.Length == 0)
                throw new System.ArgumentException("param arguments not specified");

            int firstXLength = targetZones[0].GetLength(0);
            int firstYLength = targetZones[0].GetLength(1);
            int[,] resultZone = new int[firstXLength, firstYLength];
            for (int i = 0; i < firstXLength; i++) {
                for (int j = 0; j < firstYLength; j++) {
                    resultZone[i, j] = targetZones[0][i, j];
                }
            }


            for (int x = 1; x < targetZones.Length; x++) {
                int[,] zone = targetZones[x];
                for (int i = 0; i < zone.GetLength(0); i++) {
                    for (int j = 0; j < zone.GetLength(1); j++) {
                        int zoneValue = zone[i, j];
                        if (zoneValue != 0) {
                            if (zoneValue == 3)
                                zoneValue = 2;

                            resultZone[i, j] = zoneValue;
                        }
                    }
                }
            }

            return resultZone;
        }

        /// <summary>
        /// Resets all grid cells materials to it's initial visual state
        /// </summary>
        public void ResetGridCellsAppearance() {
            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++) {
                    GridMeshBuilder.CellInfo cell = grid.GetCell(column, row);
                    cell.material.color = cell.defaultMaterial.color;
                }
            }
        }

        /// <summary>
        /// Display target zone on the arena prespective grid
        /// </summary>
        public void ShowTargetZone(Vector2Int actorPosition, int[,] targetZone, Color playerColor) {
            List<GridMeshBuilder.CellInfo> modifiedCells = new List<GridMeshBuilder.CellInfo>();

            Vector2Int actorTargetZonePosition = GetActorTargetZoneLocalPosition(targetZone);
            Vector2Int searchRange = new Vector2Int(targetZone.GetLength(0), targetZone.GetLength(1));
            Vector2Int anchoredGridSearchPosition = actorPosition - actorTargetZonePosition; // top left anchor

            //Vector2Int worldGridSearchIterator = startSearchCoord;
            Vector2Int gridBounds = new Vector2Int(actorGrid.GetLength(0), actorGrid.GetLength(1));

            for (int x = 0, x2 = anchoredGridSearchPosition.x; x < searchRange.x; x++, x2++) { // rows
                for (int y = 0, y2 = anchoredGridSearchPosition.y; y < searchRange.y; y++, y2++) { // columns

                    if (x2 >= 0 && x2 < gridBounds.x && y2 >= 0 && y2 < gridBounds.y) {
                        GridMeshBuilder.CellInfo cell = grid.GetCell(y2, x2);
                        int targetZoneValue = targetZone[x, y];

                        if (targetZoneValue == 2) {
                            //Debug.Log(x2 + " " + y2 + " = " + tzValue);
                            
                            Color color = Color.red;

                            color.a = cell.defaultMaterial.color.a;
                            cell.material.color = color;
                        }
                        else if (targetZoneValue == 1) {
                            Color color = playerColor;
                            color.a = cell.defaultMaterial.color.a;
                            cell.material.color = color;
                        }

                    }
                }

            }

        }

        /// <summary>
        /// Display target zone on the arena prespective grid
        /// </summary>
        public void ShowTargetZone(HumanoidContestant actor, Color playerColor, Skill skill = null) {
            Vector2Int actorGridPosition = GetSpotGridPositionNew(actor.teamNumber, actor.spotNumber); // x = row, y = column
            int[,] targetZone = GetContestantTargetZone(actor, skill);

            ShowTargetZone(actorGridPosition, targetZone, playerColor);
        }

        /// <summary>
        /// Highlights the cell at given column and row with passed color
        /// </summary>
        public void HighlightCell(int row, int column, Color color) {
            GridMeshBuilder.CellInfo cell = grid.GetCell(column, row);
            cell.material.color = color;
        }

        /// <summary>
        /// Unhighlights the cell at given column and row
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void UnhighlightCell(int row, int column) {
            GridMeshBuilder.CellInfo cell = grid.GetCell(column, row);
            cell.material.color = cell.defaultMaterial.color;
        }

        /// <summary>
        /// Find player coordinate on target zone.
        /// </summary>
        private Vector2Int GetActorTargetZoneLocalPosition(int[,] targetZone) {
            // Find out player coordinate
            for (int x = 0; x < targetZone.GetLength(0); x++)
                for (int y = 0; y < targetZone.GetLength(1); y++)
                    if (targetZone[x, y] == 1)
                        return new Vector2Int(x, y);

            throw new ArgumentException("Actor was not found in specified target zone");
        }

        /// <summary>
        /// Get all arena grid contestatns of given range and start search location in grid coordinates
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        private ContestantBody[,] GetArenaGridContestants(Vector2Int insideGridLocation, Vector2Int range) {
            ContestantBody[,] subArea = new ContestantBody[range.x, range.y];
            //Vector2Int worldGridSearchIterator = startSearchCoord;
            Vector2Int gridBounds = new Vector2Int(actorGrid.GetLength(0), actorGrid.GetLength(1));

            for (int x = 0, x2 = insideGridLocation.x; x < range.x; x++, x2++) { // rows
                for (int y = 0, y2 = insideGridLocation.y; y < range.y; y++, y2++) { // columns
                    if (x2 >= 0 && x2 < gridBounds.x && y2 >= 0 && y2 < gridBounds.y) {
                        subArea[x, y] = actorGrid[x2, y2];
                    }
                }
            }

            return subArea;
        }

        /// <summary>
        /// Preset handling
        /// </summary>
        private void Update() {
            ArenaGridPreset preset = null;

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                preset = presets[0];
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                preset = presets[1];
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                preset = presets[2];
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                preset = presets[3];
            }

            if (preset != null && preset != currentPreset) {
                currentPreset = preset;

                grid.topLeft = currentPreset.topLeft;
                grid.topRight = currentPreset.topRight;
                grid.bottomLeft = currentPreset.bottomLeft;
                grid.bottomRight = currentPreset.bottomRight;

                topLeft = currentPreset.topLeft;
                topRight = currentPreset.topRight;
                bottomLeft = currentPreset.bottomLeft;
                bottomRight = currentPreset.bottomRight;
                grid.z = currentPreset.depth;
                VisualContestantDirector.Instance.topDepthScale.x = topLeft.y;
                VisualContestantDirector.Instance.bottomDepthScale.x = bottomLeft.y;

                arenaBackgroundTemp.sprite = preset.perspectiveBackground;
                arenaBackgroundTemp.transform.localScale = preset.scale;
                grid.Rebuild();
                VisualContestantDirector.Instance.LeaveField();
                VisualContestantDirector.Instance.EnterField(MatchManager.Instance.teams);
                //foreach (var b in actorGrid) {
                //    if (b != null)
                //        b.transform.position = GetSpotPosition(b.GetContestant().teamNumber, b.GetContestant().spotNumber);
                //}
            }
        }

        // For debugging
        private void DebugPrintTargetZone(int[,] targetZone) {
            for (int i = 0; i < targetZone.GetLength(0); i++) {
                string msg = "row [" + i + "]: ";
                for (int j = 0; j < targetZone.GetLength(1); j++) {
                    msg += targetZone[i, j] + " ";
                }

                Debug.Log(msg);
            }
        }
    }
}
