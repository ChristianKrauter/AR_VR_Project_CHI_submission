//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;

namespace Valve.VR.Extras
{
    public class SteamVR_LaserPointer : MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public event PointerEventHandler PointerClick;
        public float visualizationTime = 1.5f;

        private Player player = null;
        private int lastState  = 0;
        Transform previousContact = null;
        private bool teleportAllowed = false;
        private Vector3 teleportPosition;
        


        private void Start()
        {
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

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerClick(PointerEventArgs e)
        {
            if (PointerClick != null)
                PointerClick(this, e);
        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
        }


    private void Update()
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
                Debug.Log("2");
                pointer.SetActive(true);
                lastState = 1;
                pointer.transform.position = pointer.transform.forward * dist / 2;
                pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);


                if (previousContact && previousContact != hit.transform)
                {
                    Debug.Log("3");
                    PointerEventArgs args = new PointerEventArgs();
                    args.fromInputSource = pose.inputSource;
                    args.distance = 0f;
                    args.flags = 0;
                    args.target = previousContact;
                    OnPointerOut(args);
                    previousContact = null;//Chris stinkt
                }

                if (bHit)
                {
                    Debug.Log("4");
                    dist = hit.distance;

                    pointer.transform.position = pointer.transform.forward * dist / 2;
                    pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
                    teleportPosition = hit.transform.gameObject.transform.position + hit.transform.gameObject.transform.forward*2f; //Chris stinkt
                    Debug.Log(teleportPosition);

                    dist = hit.distance;
                    if (hit.collider.gameObject.tag == "teleport_target")
                    {
                        Debug.Log("7");
                        pointer.GetComponent<MeshRenderer>().material.color = Color.green;
                        teleportAllowed = true;
                    }
                    else
                    {
                        Debug.Log("8");
                        pointer.GetComponent<MeshRenderer>().material.color = Color.red;
                        teleportAllowed = false;
                    }
                }
                else
                {
                    Debug.Log("5");
                    teleportAllowed = false;
                }

            }
            else
            {
                //Debug.Log("9");
                pointer.SetActive(false);
                if (lastState == 1)
                {
                    Debug.Log("released");
                    
                    if (teleportAllowed)
                    {
                        Debug.Log("teleport");
                        Debug.Log(teleportPosition);

                        player = GameObject.FindObjectOfType<Player>();
                        Debug.Log(player);

                        initiateTeleport(player,teleportPosition);
                    }
                }
                lastState = 0;
            }
            float dist2 = dist/2;
            pointer.transform.localPosition = new Vector3(0f, 0f, dist2);
        }

        private void initiateTeleport(Player player, Vector3 teleportPosition)
        {
            SteamVR_Fade.Start(Color.clear, 0);
            SteamVR_Fade.Start(Color.black, visualizationTime);
            Invoke("teleportPlayer", visualizationTime);
            
        }
        private void teleportPlayer()
        {
            SteamVR_Fade.Start(Color.clear, visualizationTime/2);
            player.transform.position = teleportPosition;
            teleportAllowed = false;
        }
    }

    
    public struct PointerEventArgs
    {
        public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);

}