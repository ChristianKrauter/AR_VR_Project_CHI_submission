//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;
using System.IO;

namespace Valve.VR.Extras
{
    public class TeleVisVrTeleport: MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform elevator;
        public Transform horizontal;
        public Transform camera;
        public Animator elevatorAnimator;
        public Animator horizontalAnimator;
        public float teleportDistanceFactor; //Default 2

        public float visualizationTime = 1.5f;
        public studyController studyController;

        public Player player = null;
        private int lastState  = 0;
        Transform previousContact = null;
        private bool teleportAllowed = false;
        private bool finish = false;
        private Vector3 teleportPosition;
        private Vector3 teleportDestinationForward;
        private Animator anim;
        private GameObject teleportTarget;
        private bool teleportInProgress = false;//CHRIS STINKT

        private void Start()
        {
            teleportTarget = new GameObject();
            if (pose == null)
                pose = this.GetComponent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                Debug.LogError("No SteamVR_Behaviour_Pose component found on this object");
            if (interactWithUI == null)
                Debug.LogError("No ui interaction action has been set on this component.");

            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
        }


    private void Update()
        {
            if (!teleportInProgress)
            {
            pointer.SetActive(true);
            pointer.GetComponent<MeshRenderer>().material.color = Color.red;
            
                
                if (!isActive)
                {
                    isActive = true;
                    this.transform.GetChild(0).gameObject.SetActive(true);
                }
                float dist = 100f;

                Ray raycast = new Ray(transform.position, transform.forward);
                RaycastHit hit;
                bool bHit = Physics.Raycast(raycast, out hit);

                if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
                {
                    pointer.SetActive(true);
                    lastState = 1;
                    pointer.transform.position = pointer.transform.forward * dist / 2;
                    pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);

                    if (bHit)
                    {
                        dist = hit.distance;

                        pointer.transform.position = pointer.transform.forward * dist / 2;
                        pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
                        teleportPosition = hit.transform.position + hit.transform.forward * teleportDistanceFactor;
                        teleportDestinationForward = hit.transform.forward;

                        dist = hit.distance;
                        if (hit.collider.gameObject.tag == "teleport_target")
                        {
                            pointer.GetComponent<MeshRenderer>().material.color = Color.green;
                            teleportTarget = hit.collider.gameObject;
                            teleportAllowed = true;
                        }
                        else if (hit.collider.gameObject.tag == "Ui")
                        {
                            pointer.GetComponent<MeshRenderer>().material.color = Color.green;
                        }
                        else if (hit.collider.gameObject.tag == "finish")
                        {
                            pointer.GetComponent<MeshRenderer>().material.color = Color.green;
                            teleportAllowed = true;
                            finish = true;
                        }
                        else
                        {
                            pointer.GetComponent<MeshRenderer>().material.color = Color.red;
                            teleportAllowed = false;
                        }
                    }
                    else
                    {
                        teleportAllowed = false;
                        finish = false;
                    }

                }
                else
                {
                    pointer.SetActive(false);
                    if (lastState == 1)
                    {
                        if (hit.collider != null)
                        {
                            if (hit.collider.gameObject.tag == "Ui")
                            {
                                teleportInProgress = true;
                                Invoke("teleportStopped", visualizationTime);
                                Debug.Log(hit.collider.gameObject.name);
                                switch (hit.collider.gameObject.name)
                                {
                                    case "instant_cube":
                                        studyController.startButton(visualizationTime, 0);
                                        break;
                                    case "fade_cube":
                                        studyController.startButton(visualizationTime, 1);
                                        break;
                                    case "vertical_cube":
                                        studyController.startButton(visualizationTime, 2);
                                        break;
                                    case "horizontal_cube":
                                        studyController.startButton(visualizationTime, 3);
                                        break;
                                    case "end_cube":
                                        studyController.endButton();
                                        break;
                                    default:
                                        studyController.startButton(visualizationTime, 0);
                                        break;
                                }
                            }
                            if (teleportAllowed)
                            {
                                teleportInProgress = true;
                                player = GameObject.FindObjectOfType<Player>();
                                initiateTeleport(player, teleportPosition);
                            }
                        }

                    }
                    lastState = 0;
                }
                float dist2 = dist / 2;
                pointer.transform.localPosition = new Vector3(0f, 0f, dist2);
            }
        }

        private void teleportStopped()
        {
            teleportInProgress = false;
        }
        private void initiateTeleport(Player player, Vector3 teleportPosition)
        {
             if(studyController.currentVisualization == 0)
            {
                
                Invoke("teleportPlayer", 0f);
            }
            else if (studyController.currentVisualization == 1)
            {
                SteamVR_Fade.Start(Color.clear, 0);
                SteamVR_Fade.Start(Color.black, visualizationTime);
                if (null != elevatorAnimator)
                {
                    elevatorAnimator.Play("mainElevator", 0, 0.0f);
                }
                Invoke("teleportPlayer", visualizationTime);
            }
            else if (studyController.currentVisualization == 2)
            {
                elevator.position = new Vector3(camera.position.x,camera.position.y-1.6f,camera.position.z);
                elevator.gameObject.SetActive(true);
                if (null != elevatorAnimator)
                {
                    elevatorAnimator.Play("mainElevator", 0, 0.0f);
                }
                Invoke("teleportPlayer", visualizationTime);
            }
            else if (studyController.currentVisualization == 3)
            {
                horizontal.position = new Vector3(camera.position.x, camera.position.y, camera.position.z);
                horizontal.transform.up = (player.transform.position- teleportPosition).normalized;
                horizontal.gameObject.SetActive(true);
                if (null != horizontalAnimator)
                {
                    horizontalAnimator.Play("mainHorizontal", 0, 0.0f);
                }
                Invoke("teleportPlayer", visualizationTime);
            } else
            {
             }
            
        }
        private void teleportPlayer()
        {
            SteamVR_Fade.Start(Color.clear, visualizationTime/2);
            if (finish)
            {
                finish = false;
                studyController.startNextRun();
            }
            else
            {
                studyController.nextStep(teleportTarget);
                player.transform.position = teleportPosition;
            }
            teleportAllowed = false;
            teleportInProgress = false;
        }
    }

}