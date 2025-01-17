﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Mode { Play, Slice, Inventory, Place, Map}
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlaneUsageExample plane;
    public Mode mode;
    //{
    //    get { return mode; }
    //    private set { mode = value; }
    //}
    public GameObject player;
    private CharacterController characterController;
    public CameraFollow camHolder;

    public AudioSource audioSource;
    public AudioClip sliceSound;

    //slice stuff
    public DefaultSliceable curSliceable;
    public GameObject selectedObject;
    public GameObject selectedSliceable;
    public int mousePresses;
    public LineRenderer cutLine;
    public PlaneUsageExample slicePlane;
    public Material crossMat;
    private Vector3 sliceDir;

    public GameObject placeObject;

    public GameObject schaarIcon;
    private RaycastHit raycastHit;

    public GameObject miniMap;


    private float lineOffset = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        player = GameObject.Find("Player");
        characterController = player.GetComponent<CharacterController>();
        audioSource = gameObject.GetComponent<AudioSource>();

    }
    void Start()
    {
        SetMode(Mode.Play);
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Play:
                PlayModeBehaviour();

                break;

            case Mode.Slice:
                SliceModBehaviour();

                break;

            case Mode.Inventory:
                InventoryModeBehaviour();

                break;

            case Mode.Place:
                PlaceModeBehaviour();

                break;

            case Mode.Map:
                MapModeBehaviour();
                break;




        }
    }
    //set mode wisselt de modes en kan een keer nieuwe waardes op elke wissel initialiseren
    public void SetMode(Mode m)
    {
        mode = m;
        switch (mode)
        {
            //initial state switch, executes once.

            case Mode.Play:
                characterController.enabled = true;
                selectedSliceable.GetComponent<cakeslice.Outline>().eraseRenderer = true;
                camHolder.target = player.transform;
                cutLine.enabled = false;
                mousePresses = 0;
                schaarIcon.SetActive(false);
                InventoryUI.instance.gameObject.SetActive(false);
                miniMap.SetActive(false);
                //initialize once something
                break;

            case Mode.Slice:
                //initialize once something
                characterController.enabled = false;
                selectedSliceable.GetComponent<cakeslice.Outline>().eraseRenderer = false;
                camHolder.target = selectedObject.transform;
                schaarIcon.SetActive(true);
                InventoryUI.instance.gameObject.SetActive(false);
                break;

            case Mode.Inventory:
                InventoryUI.instance.gameObject.SetActive(true);


                break;

            case Mode.Place:
                InventoryUI.instance.gameObject.SetActive(false);
                break;
            //initialize inventory

            case Mode.Map:
                miniMap.SetActive(true);
                break;

        }

    }

    //alles voor in de play mode
    void PlayModeBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMode(Mode.Inventory);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SetMode(Mode.Map);
        }

            if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                raycastHit = hit;
                selectedObject = hit.collider.gameObject;
                Debug.Log("You selected the " + hit.transform.name);

                if (selectedObject.GetComponent<ISliceable>() != null)
                {
                    selectedSliceable = hit.collider.gameObject;
                    var sliceable = selectedObject.GetComponent<ISliceable>();
                    slicePlane.gameObject.transform.position = hit.point;

                    //ik cache hier het sliceable object zodat we hem later kunnen aanpassen
                    curSliceable = selectedObject.GetComponent<DefaultSliceable>();
                    //set mode naar slice mode
                    SetMode(Mode.Slice);

                    //sliceable?.SliceSingleMesh(slicePlane);
                    //SliceObjectRecursive(slicePlane, hitObject, crossMat);

                }

            }
        }
    }



    //alles voor in de slice mode
    void SliceModBehaviour()
    {
        schaarIcon.transform.position = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.E))
        {
            SetMode(Mode.Play);

        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                raycastHit = hit;
                //selectedObject = hit.collider.gameObject;
                Debug.Log("You selected the " + hit.transform.name);

                if (selectedObject.GetComponent<ISliceable>() != null)
                {
                    var sliceable = selectedObject.GetComponent<ISliceable>();
                    slicePlane.gameObject.transform.position = hit.point;

                    //ik cache hier het sliceable object zodat we hem later kunnen aanpassen
                    curSliceable = selectedObject.GetComponent<DefaultSliceable>();
                    selectedObject.GetComponent<cakeslice.Outline>().eraseRenderer = false;

                    //sliceable?.SliceSingleMesh(slicePlane);
                    //SliceObjectRecursive(slicePlane, hitObject, crossMat);

                }

            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (mousePresses == 0)
            {
                //do your stuff for first time
                mousePresses++;
                cutLine.enabled = true;
                cutLine.SetPosition(0, new Vector3(raycastHit.point.x, raycastHit.point.y, selectedSliceable.transform.position.z - lineOffset));
            }
            else
            {
                //do your stuff for all other times
                cutLine.SetPosition(1, new Vector3(raycastHit.point.x, raycastHit.point.y, selectedSliceable.transform.position.z - lineOffset));
                mousePresses = 0;

                //align sliceplane rotation to slice line direction:
                sliceDir = (cutLine.GetPosition(0) - cutLine.GetPosition(1)).normalized;

                slicePlane.transform.rotation = Quaternion.LookRotation(sliceDir);

                selectedSliceable.GetComponent<ISliceable>()?.SliceSingleMesh(slicePlane);

                audioSource.PlayOneShot(sliceSound);

                SetMode(Mode.Play);

            }
        }

        if (mousePresses != 0)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                cutLine.SetPosition(1, new Vector3(hit.point.x, hit.point.y, selectedSliceable.transform.position.z - lineOffset));

            }
        }
        //curSliceable?.SliceSingleMesh(plane);
        //SetMode(Mode.Play);
    }

    void InventoryModeBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMode(Mode.Play);
        }
    }

    void PlaceModeBehaviour()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            placeObject.transform.position = hit.point;

            if(Input.GetMouseButtonDown(0))
            {

                SetMode(Mode.Play);
            }
        }
    }

    void MapModeBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetMode(Mode.Play);
        }
    }

}
