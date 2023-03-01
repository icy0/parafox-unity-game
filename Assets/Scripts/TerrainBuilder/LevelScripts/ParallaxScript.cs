using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  ----------------------------------------------------
//  |   This simple script is responsible for the
//  |   parallax effect of the sections in different
//  |   layers
//  ----------------------------------------------------
public class ParallaxScript : MonoBehaviour {

    //  ----------------------------------------------------
    //  |   Attributes
    //  ----------------------------------------------------

    private Vector3 origin;
    private Vector3 initalOffset;

    //  ----------------------------------------------------
    //  |   Set the attributes on startup
    //  ----------------------------------------------------
    void Start()
    {
        this.origin = this.transform.position;
        this.initalOffset = Camera.main.transform.position - this.origin;
        this.initalOffset.z = 0;
    }

    //  ----------------------------------------------------
    //  |   Update gets called every frame and is
    //  |   responsible for moving the section object around
    //  ----------------------------------------------------
    void Update()
    {

        // Calculate in which way the camera was moved from the origin of the object
        // minus its inital offset to the camera
        Vector3 shift = Camera.main.transform.position - this.initalOffset - this.origin;
        // Set the shift in z direction to zero, so that the layers aren't getting shifted
        // in this direction
        shift.z = 0;
        // Set the new position of the layer object as the origin plus the shift vector times
        // the strength of the parallax effect
        this.transform.position = this.origin + shift * this.getStrength();
    }

    //  ----------------------------------------------------
    //  |   Calculate the strength of the parallax effect
    //  |   based on the z position of the game object
    //  ----------------------------------------------------
    private float getStrength()
    {
        // The strength is equal the z position of the layer object and
        // is then just clamped to 1.0F and -1.0F
        return Mathf.Min(1.0F, Mathf.Max(this.origin.z, -1.0F));
    }
}
