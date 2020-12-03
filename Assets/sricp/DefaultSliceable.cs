﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class DefaultSliceable : MonoBehaviour, ISliceable
{
    public Material crossMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void SliceSingleMesh( PlaneUsageExample slicePlane)
    {

        SliceSingleMeshExtended(crossMat, slicePlane, out GameObject upperHull, out GameObject lowerHull, (x,y)=> 
        {
            Rigidbody body1 = x.AddComponent<Rigidbody>();
            MeshCollider coll1 = x.AddComponent<MeshCollider>();
            DefaultSliceable xSliceable = x.AddComponent<DefaultSliceable>();
            xSliceable.crossMat = crossMat;
            coll1.convex = true;
            body1.AddForce(new Vector3(0, 1f, Random.Range(-2, 2)), ForceMode.Impulse);

            Rigidbody body2 = y.AddComponent<Rigidbody>();
            MeshCollider coll2 = y.AddComponent<MeshCollider>();
            DefaultSliceable ySliceable = y.AddComponent<DefaultSliceable>();
            ySliceable.crossMat = crossMat;
            coll2.convex = true;
            body2.AddForce(new Vector3(0, 1f, Random.Range(-2, 2)), ForceMode.Impulse);
        });
    }

    protected void SliceSingleMeshExtended(Material crossMaterial, PlaneUsageExample slicePlane, out GameObject upperHull, out GameObject lowerHull, System.Action<GameObject, GameObject> OnDoneAction = null)
    {
        // only slice the parent object

        SlicedHull hull = slicePlane.SliceObject(gameObject, crossMaterial);

        lowerHull = null;
        upperHull = null;

        if (hull != null)
        {
            lowerHull = hull.CreateLowerHull(gameObject, crossMaterial);
            upperHull = hull.CreateUpperHull(gameObject, crossMaterial);
     
            gameObject.SetActive(false);

            OnDoneAction?.Invoke(upperHull, lowerHull);
        }


    }
}
