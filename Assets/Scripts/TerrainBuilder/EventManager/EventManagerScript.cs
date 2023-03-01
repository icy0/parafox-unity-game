using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class EventManagerScript : MonoBehaviour {

    [SerializeField]
    public List<EventGroup> eventGroups;

    [SerializeField]
    public string eventGroupSaveFilePath;


    void Start() {

        this.eventGroups = new List<EventGroup>();

        if (this.eventGroupSaveFilePath.Equals("")) {
            Debug.LogError("No event group save file was saved. No Events will be executed.");
            return;
        }

        string fileSystemPath = Application.dataPath.Replace('/', '\\');
        fileSystemPath = fileSystemPath.Insert(2, "\\") + "\\" + this.eventGroupSaveFilePath;

        XMLReader xmlReader = new XMLReader(fileSystemPath);
        xmlReader.evaluate();

        this.createEventManagementSystem(xmlReader);
    }

    private void createEventManagementSystem(XMLReader xmlReader) {

        Tuple<string, string>[] emsAttr = xmlReader.attributes[XMLReader.AssembleXPath(Level.LEVEL_TAG, Level.EVENT_MANAGEMENT_SYSTEM_TAG)];
        int egCount = int.Parse(emsAttr[0].Item2);

        for (int i = 0; i < egCount; ++i) {
            this.eventGroups.Add(this.createEventGroup(xmlReader, i));
        }

    }

    private EventGroup createEventGroup(XMLReader xmlReader, int index) {

        string eventGroupTag = Level.EVENT_GROUP_TAG + index.ToString();
        string eventGroupInformationLine = xmlReader.values[XMLReader.AssembleXPath(Level.LEVEL_TAG, Level.EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, Level.EVENT_GROUP_INFORMATION_TAG)][0];
        string[] eventGroupInformation = CSVHandler.read(eventGroupInformationLine);

        EventGroup eg = new EventGroup(eventGroupInformation[0]);

        Tuple<string, string>[] egnAttr = xmlReader.attributes[XMLReader.AssembleXPath(Level.LEVEL_TAG, Level.EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, Level.EVENT_GROUP_NODES_TAG)];
        int egnCount = int.Parse(egnAttr[0].Item2);

        for (int i = 0; i < egnCount; ++i) {
            string nodeTag = Level.NODE_TAG + i.ToString();
            string nodeLine = xmlReader.values[XMLReader.AssembleXPath(Level.LEVEL_TAG, Level.EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, Level.EVENT_GROUP_NODES_TAG, nodeTag)][0];

            string[] nodeInformation = CSVHandler.read(nodeLine);
            int levelObjectID = int.Parse(nodeInformation[0]);

            GameObject responsibleObject = null;

            Transform levelObjectParent = GameObject.Find("LevelObjects").transform;

            for (int j = 0; j < levelObjectParent.childCount; ++j) {
                GameObject o = levelObjectParent.GetChild(j).gameObject;
                int id = o.GetComponent<LevelObjectScript>().id;
                if (id == levelObjectID) {
                    responsibleObject = o;
                    break;
                }
            }

            if (responsibleObject == null) {
                continue;
            }

            responsibleObject.GetComponent<Node>().configureNode(nodeInformation);
            Node n = responsibleObject.GetComponent<Node>();
            eg.nodes.Add(n);
        }

        eg.sort();

        for (int i = 0; i < egnCount; ++i) {
            string nodeTag = Level.NODE_TAG + i.ToString();
            string nodeLine = xmlReader.values[XMLReader.AssembleXPath(Level.LEVEL_TAG, Level.EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, Level.EVENT_GROUP_NODES_TAG, nodeTag)][0];

            string[] nodeInformation = CSVHandler.read(nodeLine);
            int predecessorNodeCount = int.Parse(nodeInformation[5]);
            for (int j = 0; j < predecessorNodeCount; ++j) {
                int predecessorID = int.Parse(nodeInformation[6 + j]);
                foreach(Node n in eg.nodes) {
                    if (n.GetComponent<Node>().id == predecessorID) {
                        eg.nodes[i].predecessors.Add(n);
                        break;
                    }
                }
            }
        }

        return eg;
    }

    void Update() {
        foreach(EventGroup eg in this.eventGroups) {
            eg.update();
        }
    }
}