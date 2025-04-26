using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private struct Cell {
        public WorldGeneratorNode node;
        public Vector3Int offsetWithinNode;
        public void SetNode(WorldGeneratorNode node, Vector3Int offsetWithinNode) {
            this.node = node;
            this.offsetWithinNode = offsetWithinNode;
        }
        public void ClearNode() {
            this.node = null;
        }
        public bool HasNode() {
            return this.node != null;
        }
    }

    [SerializeField] private Vector3Int sizeInCells;
    [SerializeField] private Vector3Int sizeOfEachCell;
    [SerializeField] private GameObject parentOfNodesToUse;
    [SerializeField] private int maxGenerationAttempts = 100;
    [SerializeField] private float targetPopulationFactor = 0.0001f;
    [SerializeField] private float timeout = 10.0f;
    private WorldGeneratorNode[] nodePalette;
    private int[] paletteConsultationOrder;
    private WorldGeneratorNode[] uniqueNodePalette;
    private int[] uniquePaletteConsultationOrder;
    private int[] quarterTurnConsultationOrder;
    private Cell[] grid;
    private float startTime;

    void Start() {
        CreateMemberArrays();
        Generate();
    }

    private void EnforceTimeout(WorldGeneratorNode cleanup = null) {
        if (Time.realtimeSinceStartup - startTime >= timeout) {
            if (cleanup != null) {
                Destroy(cleanup.gameObject);
            }
            throw new ExceptionAbout<WorldGenerator>("Timed out");
        }
    }

    private void CreateMemberArrays() {
        int childCount = parentOfNodesToUse.transform.childCount;
        int paletteSize = 0;
        int uniquePaletteSize = 0;
        for (int i = 0; i < childCount; i++) {
            var node =
                parentOfNodesToUse.transform.GetChild(i).GetComponent<WorldGeneratorNode>();
            if (node.unique) {
                uniquePaletteSize += node.frequency;
            } else {
                paletteSize += node.frequency;
            }
        }
        nodePalette = new WorldGeneratorNode[paletteSize];
        paletteConsultationOrder = new int[paletteSize];
        uniqueNodePalette = new WorldGeneratorNode[uniquePaletteSize];
        uniquePaletteConsultationOrder = new int[uniquePaletteSize];
        quarterTurnConsultationOrder = new int[] {0, 1, 2, 3};
        grid = new Cell[sizeInCells.x*sizeInCells.y*sizeInCells.z];
        int nodeID = 0;
        int uniqueNodeID = 0;
        for (int i = 0; i < childCount; i++) {
            var node =
                parentOfNodesToUse.transform.GetChild(i).GetComponent<WorldGeneratorNode>();
            for (int j = 0; j < node.frequency; j++) {
                if (node.unique) {
                    uniqueNodePalette[uniqueNodeID] = node;
                    uniquePaletteConsultationOrder[uniqueNodeID] = uniqueNodeID;
                    uniqueNodeID++;
                } else {
                    nodePalette[nodeID] = node;
                    paletteConsultationOrder[nodeID] = nodeID;
                    nodeID++;
                }
            }
        }
    }

    private int GetGridIndex(Vector3Int v) {
        if (
            v.x < 0 || v.y < 0 || v.z < 0 ||
            v.x >= sizeInCells.x ||
            v.y >= sizeInCells.y ||
            v.z >= sizeInCells.z
        ) {
            return -1;
        } else {
            return (v.x*sizeInCells.y + v.y)*sizeInCells.z + v.z;
        }
    }

    private Vector3 GetCellPosition(Vector3Int v) {
        return transform.position + Vector3.Scale(sizeOfEachCell, new Vector3(
            ((float) v.x) - ((float) sizeInCells.x)/2.0f,
            ((float) v.y) - ((float) sizeInCells.y)/2.0f,
            ((float) v.z) - ((float) sizeInCells.z)/2.0f
        ));
    }

    private bool TrySetNode(Vector3Int v, WorldGeneratorNode node) {
        EnforceTimeout(node);
        var added = new List<int>();
        for (int x = 0; x < node.sizeInCells.x; x++) {
            EnforceTimeout(node);
            for (int y = 0; y < node.sizeInCells.y; y++) {
                EnforceTimeout(node);
                for (int z = 0; z < node.sizeInCells.z; z++) {
                    EnforceTimeout(node);
                    var u = new Vector3Int(x, y, z);
                    int i = GetGridIndex(v + u);
                    if (i < 0 || grid[i].HasNode()) {
                        foreach (int j in added) {
                            grid[j].ClearNode();
                        }
                        return false;
                    } else {
                        grid[i].SetNode(node, u);
                        added.Add(i);
                    }
                }
            }
        }
        node.transform.position = GetCellPosition(v) +
            Vector3.Scale(sizeOfEachCell, ((Vector3) node.sizeInCells)/2.0f);
        node.transform.SetParent(transform, true);
        return true;
    }

    private bool TrySetNodeAnywhere(WorldGeneratorNode node) {
        EnforceTimeout(node);
        for (int i = 0; i < maxGenerationAttempts; i++) {
            EnforceTimeout(node);
            var posn = new Vector3Int(
                ((int) (UnityEngine.Random.value*sizeInCells.x))%sizeInCells.x,
                ((int) (UnityEngine.Random.value*sizeInCells.y))%sizeInCells.y,
                ((int) (UnityEngine.Random.value*sizeInCells.z))%sizeInCells.z
            );
            int inverseTurn = 0;
            Misc.Shuffle(quarterTurnConsultationOrder);
            foreach (var quarterTurn in quarterTurnConsultationOrder) {
                EnforceTimeout(node);
                node.Rotate(inverseTurn);
                node.Rotate(quarterTurn);
                inverseTurn = (4 - quarterTurn)%4;
                if (TrySetNode(posn, node)) {
                    return true;
                }
            }
        }
        return false;
    }

    private WorldGeneratorNode TrySetAnyNodeAnywhere() {
        EnforceTimeout();
        Misc.Shuffle(paletteConsultationOrder);
        foreach (int nodeID in paletteConsultationOrder) {
            EnforceTimeout();
            var node = Instantiate(nodePalette[nodeID]);
            if (TrySetNodeAnywhere(node)) {
                return node;
            } else {
                Destroy(node.gameObject);
            }
        }
        return null;
    }

    private void ClearGrid() {
        for (int i = 0; i < grid.Length; i++) {
            if (grid[i].HasNode()) {
                Destroy(grid[i].node.gameObject);
            }
            grid[i].ClearNode();
        }
    }

    private float PopulationFactor() {
        int count = 0;
        for (int i = 0; i < grid.Length; i++) {
            if (grid[i].HasNode()) {
                count++;
            }
        }
        return ((float) count)/(float) grid.Length;
    }

    private bool TryGenerate() {
        EnforceTimeout();
        Misc.Shuffle(uniquePaletteConsultationOrder);
        foreach (int uniqueNodeID in uniquePaletteConsultationOrder) {
            EnforceTimeout();
            var node = Instantiate(uniqueNodePalette[uniqueNodeID]);
            if (!TrySetNodeAnywhere(node)) {
                Destroy(node.gameObject);
                return false;
            }
        }
        while (PopulationFactor() < targetPopulationFactor) {
            EnforceTimeout();
            if (!TrySetAnyNodeAnywhere()) {
                return false;
            }
        }
        return true;
    }

    private void Generate() {
        startTime = Time.realtimeSinceStartup;
        EnforceTimeout();
        for (int i = 0; i < maxGenerationAttempts - 1; i++) {
            EnforceTimeout();
            if (TryGenerate()) {
                return;
            } else {
                ClearGrid();
            }
        }
        if (!TryGenerate()) {
            throw new ExceptionAbout<WorldGenerator>("Generation failed");
        }
    }
}
