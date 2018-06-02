using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Colosseum {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GridMeshBuilder : MonoBehaviour {
        [SerializeField] private int rows = 4; // vertical leght of the grid
        [SerializeField] private int columns = 2; // horizontal leght of the grid
        [SerializeField] public Vector2 topLeft;
        [SerializeField] public Vector2 bottomLeft;
        [SerializeField] public Vector2 topRight;
        [SerializeField] public Vector2 bottomRight;
        [SerializeField] private Material cellMaterial;
        [SerializeField, HideInInspector] private MeshRenderer meshRenderer;
        [SerializeField, HideInInspector] private MeshFilter meshFilter;
        [SerializeField] public float z;

        /// <summary>
        /// Get cell information at given coordinate.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public CellInfo GetCell(int column, int row) {
            CellInfo cell;
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;

            int searchIndex = 4 * (column * 4) + (row * 4);
            //if (searchIndex >= vertices.Length - 5)
                //throw new System.ArgumentOutOfRangeException();

            cell.bounds = new Vector2[4];
            cell.bounds[0] = vertices[searchIndex];
            cell.bounds[1] = vertices[searchIndex + 1];
            cell.bounds[2] = vertices[searchIndex + 2];
            cell.bounds[3] = vertices[searchIndex + 3];
            cell.coordinates = new Vector2Int(column, row);

            cell.index = (column * rows) + row;
            cell.material = meshRenderer.materials[cell.index];
            cell.defaultMaterial = new Material(cellMaterial);

            return cell;
        }

        /// <summary>
        /// Rebuild the mesh
        /// </summary>
        public void Rebuild() {
            meshFilter.mesh = null;
            meshRenderer.materials = new Material[] { };

            Vector3[] gridCorners = new Vector3[] { topLeft, topRight, bottomLeft, bottomRight };

            Mesh gridMesh = GenerateGridMesh(columns, rows, gridCorners);
            meshFilter.mesh = gridMesh;
            meshRenderer.materials = CreateGridMaterials(cellMaterial, columns, rows);
        }

        private void Reset() {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start() {
            Vector3[] gridCorners = new Vector3[] { topLeft, topRight, bottomLeft, bottomRight };

            Mesh gridMesh = GenerateGridMesh(columns, rows, gridCorners);
            meshFilter.mesh = gridMesh;
            meshRenderer.materials = CreateGridMaterials(cellMaterial, columns, rows);
        }

        /// <summary>
        /// Generate vertex graph based on current corners rows and columns
        /// </summary>
        /// <param name="columnLength"></param>
        /// <param name="rowLength"></param>
        /// <param name="gridWorldCorners"></param>
        /// <returns></returns>
        private Vector3[,] GenerateGridVertexGraph(int columnLength, int rowLength, Vector3[] gridWorldCorners) {
            Vector3[,] gridVertexGraph = new Vector3[columnLength, rowLength];

            if (gridWorldCorners.Length < 4|| 
                rowLength == 0 || 
                columnLength == 0)
                return gridVertexGraph;

            // Create vertex graph
            //Debug.Log(columnLength);
            //Debug.Log(rowLength);
            //foreach (var item in gridWorldCorners) {
            //    Debug.Log(item);
            //}

            Vector2 position = transform.position;
            for (int x = 0; x < columnLength; x++)
                for (int y = 0; y < rowLength; y++) {
                    //Debug.Log("row " + y);
                    float columnStepPercent = (float)x / (columnLength - 1);
                    float rowStepPercent = (float)y / (rowLength - 1);
                    float nextRowStepPercent = (float)(y + 1) / (rowLength - 1);

                    if (y < rowLength - 1)
                        rowStepPercent *= z * rowStepPercent / nextRowStepPercent;

                    float topColumnStep = Mathf.Lerp(topLeft.x, topRight.x, columnStepPercent);
                    float bottomColumnStep = Mathf.Lerp(bottomLeft.x, bottomRight.x, columnStepPercent);
                    float columnStep = Mathf.Lerp(topColumnStep, bottomColumnStep, rowStepPercent);

                    float topRowStep = Mathf.Lerp(topLeft.y, bottomLeft.y, rowStepPercent);
                    float bottomRowStep = Mathf.Lerp(topRight.y, bottomRight.y, rowStepPercent);
                    float rowStep = Mathf.Lerp(topRowStep, bottomRowStep, columnStepPercent);

                    Vector2 vertex = new Vector2(columnStep, rowStep);

                    gridVertexGraph[x,y] = vertex + position;
                }

            return gridVertexGraph;
        }

        /// <summary>
        /// Converts vertex graph to on dimensional vertex array
        /// </summary>
        /// <param name="gridVertexGraph"></param>
        /// <returns></returns>
        private Vector3[] CreateGridVertexGraphArray(Vector3[,] gridVertexGraph) {
            int columnCount = gridVertexGraph.GetLength(0);
            int rowCount = gridVertexGraph.GetLength(1);

            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < columnCount - 1; i++) {
                for (int j = 0; j < rowCount - 1; j++) {
                    for (int k = 0; k < 2; k++) {
                        vertices.Add(gridVertexGraph[i + k, j]);
                        vertices.Add(gridVertexGraph[i + k, j + 1]);
                    }
                }
            }
            Vector3[] gridVertexArray = new Vector3[vertices.Count];

            for (int i = 3; i < vertices.Count; i += 4) {
                gridVertexArray[i - 3] = vertices[i - 3];
                gridVertexArray[i - 2] = vertices[i - 1];
                gridVertexArray[i - 1] = vertices[i - 2];
                gridVertexArray[i] = vertices[i];
            }

            return gridVertexArray;
        }

        /// <summary>
        /// Calulates needed triangle submeshes based on rows and columns
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        private void CalculateGridQuadSubmeshTriangles(Mesh mesh, int columnCount, int rowCount) {
            mesh.subMeshCount = columnCount * rowCount;

            for (int i = 0, vertexIndex = 0; i < mesh.subMeshCount; i++, vertexIndex += 4) {
                mesh.SetTriangles(new int[] {
                        vertexIndex, vertexIndex+1, vertexIndex+2,
                        vertexIndex+1, vertexIndex+3, vertexIndex+2 },
                        i);
            }
        }

        /// <summary>
        /// Generate uv's of the mesh based on vertex count and submesh count
        /// </summary>
        /// <param name="gridVertexCount"></param>
        /// <returns></returns>
        private Vector2[] CreateGridUV(int gridVertexCount) {
            Vector2[] uvs = new Vector2[gridVertexCount];

            for (int i = 3; i < gridVertexCount; i+=4) {
                uvs[i - 3] = Vector2.zero;
                uvs[i - 2] = new Vector2(1, 0);
                uvs[i - 1] = new Vector2(0, 1);
                uvs[i] = Vector2.one;
            }

            return uvs;
        }

        /// <summary>
        /// Create materials of the grid based on submesh count
        /// </summary>
        /// <param name="template"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        private Material[] CreateGridMaterials(Material template, int columnCount, int rowCount) {
            int materialCount = columnCount * rowCount;
            Material[] materials = new Material[materialCount];

            for (int i = 0; i < materialCount; i++) {
                materials[i] = new Material(template);
            }

            return materials;
        }

        /// <summary>
        /// Generates grid mesh based on properties
        /// </summary>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        /// <param name="gridWorldCorners"></param>
        /// <returns></returns>
        private Mesh GenerateGridMesh(int columnCount, int rowCount, Vector3[] gridWorldCorners) {
            if (columnCount <= 0 || rowCount <= 0)
                return null;

            Mesh mesh = new Mesh();
            Vector3[,] gridVertexGraph;
            int columnLength = columnCount + 1;
            int rowLength = rowCount + 1;

            gridVertexGraph = GenerateGridVertexGraph(columnLength, rowLength, gridWorldCorners);
            mesh.vertices = CreateGridVertexGraphArray(gridVertexGraph);
            CalculateGridQuadSubmeshTriangles(mesh, columnCount, rowCount);
            mesh.uv = CreateGridUV(mesh.vertices.Length);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw preview grid
        /// </summary>
        private void DrawMeshPreviewGizmo() {
            Color origGizmoColor = Gizmos.color;
            Gizmos.color = Color.white;

            Vector3[] gridWorldCorners = new Vector3[] { topLeft, topRight, bottomLeft, bottomRight };
            Mesh meshPreview = GenerateGridMesh(columns, rows, gridWorldCorners);
            Gizmos.DrawWireMesh(meshPreview);

            Gizmos.color = origGizmoColor;
        }

        private void OnDrawGizmos() {
            DrawMeshPreviewGizmo();
            //Mesh gridMeshPreview = BuildMesh();
            //if (gridMeshPreview == null)
            //    return;


            //foreach (var item in gridVertexGraph) {
            //    Gizmos.DrawWireSphere(item, .1f);
            //}

            //Debug.Log(gridVertexArray.Length);
            //foreach (var item in gridVertexArray) {
            //    Gizmos.DrawWireSphere(item, .1f);
            //}

            //for (int i = 0; i < gridVertexGraph; i++) {

            //}

            //for (int x = 0; x < columns; x+=2) {
            //    for (int y = 0; y < rows+1; y++) {
            //        vertices.Add(gridVertexGraph[x, y]);
            //        vertices.Add(gridVertexGraph[x + 1, y]);
            //    }
            //}


            //int xDir = 1;
            //int yDir = 1;
            //int x = 0;
            //int y = 0;
            //while (x < vertexLayout.GetLength(0)) {
            //    unorderedVertices.Add(vertexLayout[x, y]);

            //    if (xDir == 0 && yDir == 0) {
            //        xDir = 1;
            //        yDir = -1;
            //    }
            //    else if (xDir == 1 && yDir == 0) {
            //        xDir = -1;
            //        yDir = 1;
            //    }
            //    else if (xDir == -1 && yDir == 1) {
            //        xDir = 1;
            //        yDir = 1;
            //    }
            //    else if (xDir == 1 && yDir == 1) {
            //        xDir = 0;
            //        yDir = 0;
            //    }

            //}

            //for (int x = 0; x < vertexLayout.GetLength(0); x+=xDir) {
            //    Debug.Log("column: " + x);
            //    for (int y = 0; y < vertexLayout.GetLength(1); y+=yDir) {
            //        Debug.Log("row: " + y);
            //        unorderedVertices.Add(vertexLayout[x, y]);
            //        yDir = 
            //    }
            //}
            //Debug.Log(vertices.Count);
            //Debug.Log(gridVertexGraph.Length);
            //Debug.Assert(vertices.Count == gridVertexGraph.Length);

            //int sideRowVertexCount = (rows + 1) * 2 - 2; // 8
            //unorderedVertices.RemoveAt(0);
            //unorderedVertices.RemoveAt(sideRowVertexCount - 1);
            //unorderedVertices.RemoveAt(vertexCount - 1);
            //unorderedVertices.RemoveAt(vertexCount - sideRowVertexCount);

            //for (int i = 0; i < unorderedVertices.Count; i++) {
            //    vertices[i]
            //}

            //unorderedVertices.RemoveAt((rows + 1) * 2 - 2);

            //foreach (var v in vertices) {
            //    Gizmos.DrawWireSphere(v, 0.1f);
            //}
        }
#endif

        /// <summary>
        /// Helper class to get information about cells of the grid mesh
        /// </summary>
        // Represents information abot cell given grid coordinate
        public struct CellInfo : System.IEquatable<CellInfo> {
            public int index;
            public Vector2Int coordinates;
            public Material defaultMaterial;
            public Material material;
            public Vector2[] bounds;

            /// <summary>
            /// The position/center of the cell
            /// </summary>
            public Vector2 Position {
                get {
                    Vector2 center = Vector3.zero;

                    for (int i = 0; i < bounds.Length; i++) {
                        center += bounds[i];
                    }

                    return center /= bounds.Length;
                }
            }

            public bool Equals(CellInfo other) {
                return index == other.index;
            }
        }

    }

}
