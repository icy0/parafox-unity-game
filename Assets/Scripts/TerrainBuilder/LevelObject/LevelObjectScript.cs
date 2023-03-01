using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

public interface IConfigurator {
    void configureLevelObject(string[] attributes, Level level);
}

/**
 * The level object script functions as a base class for every other level object.
 * A valid level object needs a component that is a derivative of this class. It
 * should override the serialize method, if the level object needs to save
 * additional information to the asset's path, the level object scripts type, the
 * id, the position and rotation. If any more information is stored, the 
 * "configureLevelObject" method must be overridden too. It is used to restore the
 * level objects configuration from a set of strings, which are read from the
 * level's xml file. It should be noted that both methods in the derivative class
 * should call the implementation of these methods in the base class to guarantee
 * that the level objects basic attributes are saved and read correctly.
 */

[ExecuteInEditMode]
public class LevelObjectScript : MonoBehaviour, IConfigurator {

    //  ----------------------------------------------------
    //  |   Static Attributes
    //  ----------------------------------------------------

    // A static id counter for every derivative of the LevelObjectScript class
    public static int ID = 0;

    //  ----------------------------------------------------
    //  |   Attributes
    //  ----------------------------------------------------

    // The unique id of a level object
    public int id;

    public string assetPath { get; set; }
    
    //  ----------------------------------------------------
    //  |   Static Methods
    //  ----------------------------------------------------

    //  ----------------------------------------------------
    //  |   Method to convert three string values into a
    //  |   Vector3 object, mostly used as the position
    //  ----------------------------------------------------
    protected static Vector3 ToPosition(string x, string y, string z) {
        return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    }

    //  ----------------------------------------------------
    //  |   Method to convert four string values into a
    //  |   Quaternion object, mostly used as the rotation
    //  ----------------------------------------------------
    protected static Quaternion ToRotation(string x, string y, string z, string w) {
        return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
    }

    //  ----------------------------------------------------
    //  |   Methods
    //  ----------------------------------------------------

    public void initialize() {
        this.id = ID++;
    }

    //  ----------------------------------------------------
    //  |   Configure the basic attributes of the level
    //  |   object with a given array of strings. 
    //  |   The string array is built up as follows:
    //  |   [0]     :   The index of the path in the list
    //  |                   of all asset paths
    //  |   [1]     :   The type of this level object (The
    //  |                   class name of this script)
    //  |   [2]     :   The unique id
    //  |   [3-5]   :   The objects x, y and z coordinates
    //  |   [6-9]   :   The objects rotation represented by
    //  |                   four components x, y, z, and w
    //  ----------------------------------------------------
    public virtual void configureLevelObject(string[] attributes, Level level) {
        this.assetPath = level.levelObjectAssetsPaths[int.Parse(attributes[0])];
        this.id = int.Parse(attributes[2]);
        this.transform.position = LevelObjectScript.ToPosition(attributes[3], attributes[4], attributes[5]);
        this.transform.rotation = LevelObjectScript.ToRotation(attributes[6], attributes[7], attributes[8], attributes[9]);
        try {
            this.transform.localScale = LevelObjectScript.ToPosition(attributes[10], attributes[11], attributes[12]);
        } catch (Exception) {}
    }

    // protected abstract string getLevelObjectScript();

    //  ----------------------------------------------------
    //  |   Write the level objects configuration into a
    //  |   string, that seperates its values with
    //  |   semicolons.
    //  |   The values are stored in the following order:
    //  |   [0]     :   The index of the path in the list
    //  |                   of all asset paths
    //  |   [1]     :   The type of this level object (The
    //  |                   class name of this script)
    //  |   [2]     :   The unique id
    //  |   [3-5]   :   The objects x, y and z coordinates
    //  |   [6-9]   :   The objects rotation represented by
    //  |                   four components x, y, z, and w
    //  ----------------------------------------------------
    public virtual string serialize(Level level) {
        return level.levelObjectAssetsPaths.IndexOf(this.assetPath) + ";" +
            this.GetType().Name + ";" +
            this.id + ";" +
            this.transform.position.x + ";" + this.transform.position.y + ";" + this.transform.position.z + ";" +
            this.transform.rotation.x + ";" + this.transform.rotation.y + ";" + this.transform.rotation.z + ";" + this.transform.rotation.w + ";" +
            this.transform.localScale.x + ";" + this.transform.localScale.y + ";" + this.transform.localScale.z + ";";
    }
}