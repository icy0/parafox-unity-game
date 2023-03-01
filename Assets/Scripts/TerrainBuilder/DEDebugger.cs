using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEDebugger {
    
    public static void printList<T>(string listName, List<T> list) {
        string values = listName + " [";
        foreach (T value in list) {
            values += " " + value + ";";
        }
        if (list.Count > 0) {
            values = values.Substring(0, values.Length - 1);
        }
        values += " ]";

        Debug.Log(values);
    }
}
