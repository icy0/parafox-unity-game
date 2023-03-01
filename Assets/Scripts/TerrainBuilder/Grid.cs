using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public static class Grid {

    //  ----------------------------------------------------
    //  |   STATIC ATTRIBUTES
    //  ----------------------------------------------------

    public static float CELL_SIZE = 0.5F;

    //  ----------------------------------------------------
    //  |   Get the nearest cell at the given Vector2 
    //  |   "position" with the layer "layer"
    //  ----------------------------------------------------

    public static Vector2 getSelectedCell(Vector2 position, bool isMouse) {

        Vector3 worldPos = position;
        if (isMouse) {
            Ray posRay = HandleUtility.GUIPointToWorldRay(position);
            worldPos = posRay.origin - posRay.direction * (posRay.origin.z / posRay.direction.z);
        }
        float cellX = (int) (worldPos.x / CELL_SIZE) * CELL_SIZE + ((int) ((worldPos.x % CELL_SIZE) / (CELL_SIZE / 2.0))) * CELL_SIZE;
        float cellY = (int) (worldPos.y / CELL_SIZE) * CELL_SIZE + ((int) ((worldPos.y % CELL_SIZE) / (CELL_SIZE / 2.0))) * CELL_SIZE;

        Vector2 cellPos = new Vector2(cellX, cellY);
        return cellPos;
    }
}
