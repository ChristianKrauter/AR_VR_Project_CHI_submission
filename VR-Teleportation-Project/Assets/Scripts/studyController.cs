using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class studyController : MonoBehaviour
{

    public int participantID = 1;
    //public int startCondition = 1;
    public Animator elevatorAnimator;
    public Animator horizontalAnimator;
    public Transform elevator;
    public Transform camera;
    public Transform horizontal;
    public GameObject player;
    public Valve.VR.InteractionSystem.Player LogPlayer;
    public int currentVisualization;
    public Canvas highScoreCanvas;
    public TMPro.TextMeshProUGUI highscoretable;
    public TMPro.TextMeshProUGUI playerScore;

    private int levelCount = 0;
    private string nextScene = "menu";
    private List<List<int>> balancedLatinSquare = new List<List<int>>();
    public bool tutorial = true;
    private List<GameObject> plattforms = new List<GameObject>();
    private GameObject lastLastPlattform;
    private GameObject lastPlattform;
    private GameObject currentPlattform;
    private GameObject targetPlattform;
    private GameObject nextPlattform;
    private GameObject nextNextPlattform;
    private bool setupPlattforms = true;
    private GameObject last;
    private GameObject lastLast;
    private GameObject next;
    private GameObject nextNext;
    private bool pause = true;
    private float elapsedTime;
    private List<float> highscores;
    private float playerTime;

    private void Start()
    {
        highscores = new List<float>();
        highScoreCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("space");
            nextStep(new GameObject());
        }
    }

    public void startButton(float visualizationTime, int visualization)
    {
        currentVisualization = visualization;
        pause = false;
        nextScene = "IntroductionLevel";

        if (visualization == 0)
        {
            Invoke("changeScene", 0f);
        }
        else if (visualization == 1)
        {
            SteamVR_Fade.Start(Color.clear, 0);
            SteamVR_Fade.Start(Color.black, visualizationTime);
            Invoke("changeScene", visualizationTime);

        }
        else if (visualization == 2)
        {
            elevator.position = new Vector3(camera.position.x, camera.position.y - 1.6f, camera.position.z);
            elevator.gameObject.SetActive(true);
            if (null != elevatorAnimator)
            {
                elevatorAnimator.Play("mainElevator", 0, 0.0f);
                Invoke("changeScene", visualizationTime);
            }
        }
        else if (visualization == 3)
        {
            horizontal.position = new Vector3(camera.position.x, camera.position.y, camera.position.z);
            horizontal.transform.up = (camera.transform.position - (new Vector3(0, 4, 8))).normalized;
            horizontal.gameObject.SetActive(true);
            if (null != horizontalAnimator)
            {
                horizontalAnimator.Play("mainHorizontal", 0, 0.0f);
                Invoke("changeScene", visualizationTime);
            }
        }
    }

    public void endButton() {
        SceneManager.LoadScene("MenuPlay_repeat");
    }


    void changeScene()
    {
        if (nextScene == "IntroductionLevel")
        {
            highScoreCanvas.gameObject.SetActive(false);

        }
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        Invoke("fadeIn", 1.0f);
    }

    void fadeIn()
    {
         SteamVR_Fade.Start(Color.clear, 0.0f);
    }

    public void startNextRun()
    {
        if (tutorial)
        {
            nextScene = "Level01";
            changeScene();
            tutorial = false;
            Invoke("firstStep", 0.1f);
            player.transform.position = new Vector3(0, 1.73f, 0);
            elapsedTime = Time.time;
        }
        else
        {
            //End
            pause = true;
            nextScene = "EndMenu";
            player.transform.position = new Vector3(0, 1.73f, 0);
            changeScene();

            playerTime = Time.time - elapsedTime;
            Debug.Log(elapsedTime);
            Debug.Log(Time.time);
            highscores.Add(playerTime);
            highscores.Sort();
            Debug.Log("3");
            Debug.Log(highscores.Count);
            List<float> hstemp = new List<float>();
            for (int i = 0;i<highscores.Count && i < 5; i++)
            {
                hstemp.Add(highscores[i]);
            }
            highscores = hstemp;
            Debug.Log("4");
            levelCount = 0;
            plattforms = new List<GameObject>();
            next = new GameObject();
            nextNext = new GameObject();
            last = new GameObject();
            lastLast = new GameObject();
            setupPlattforms = true;
            GameObject.Find("VRCamera").GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("level"));
            Debug.Log(playerTime);
            Debug.Log(highscores);
            
            string highscoreText = "";
            for (int i = 0; i < highscores.Count; i++)
            {
                Debug.Log(highscores[i]);

                System.TimeSpan t = System.TimeSpan.FromSeconds(highscores[i]);

                highscoreText += (i+1) + ".  " + t.Minutes + ":" + t.Seconds + ":" + t.Milliseconds + "\n";
                Debug.Log(highscoreText);
            }
            tutorial = true;
            System.TimeSpan pt = System.TimeSpan.FromSeconds(playerTime);

            string playerScoreText = pt.Minutes + ":" + pt.Seconds + ":" + pt.Milliseconds;
            playerScore.text = playerScoreText;
            highscoretable.text = highscoreText;
            highScoreCanvas.gameObject.SetActive(true);

        }
    }
   private List<GameObject> sortPlatforms(List<GameObject> platforms)
    {
        Debug.Log("1");
        List<GameObject> sortedPlatforms = new List<GameObject>(new GameObject[21]);
        foreach (GameObject go in platforms)
        {
            string name = go.name.ToString();
            int index = System.Int32.Parse(name) -1;
            sortedPlatforms[index] = go;
        }
        Debug.Log("2");
        return platforms;
    }
    GameObject[] FindObsWithTag(string tag)
    {
        GameObject[] foundObs = GameObject.FindGameObjectsWithTag(tag);
        System.Array.Sort(foundObs, CompareObNames);
        return foundObs;
    }


    int CompareObNames(GameObject x, GameObject y)
    {
        return x.name.CompareTo(y.name);
    }
    public void nextStep(GameObject teleportTarget)
    {
         if (!tutorial)
        {
            if (setupPlattforms)
            {
                plattforms = new List<GameObject>(FindObsWithTag("level"));
                GameObject[] tempPlattform = GameObject.FindGameObjectsWithTag("level");
                foreach (GameObject go in tempPlattform)
                {
                    plattforms.Add(go);
                    go.SetActive(false);
                }
                
                //plattforms.OrderBy(x => x.name).ToList();
                last = GameObject.Find("LAST");
                lastLast = GameObject.Find("LASTLAST");
                next = GameObject.Find("NEXT");
                nextNext = GameObject.Find("NEXTNEXT");
                setupPlattforms = false;
                GameObject.Find("VRCamera").GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("level");
            }
            if(currentPlattform.transform != teleportTarget.transform.parent)
            {
                last.SetActive(false);
                lastLast.SetActive(false);
                next.SetActive(false);
                nextNext.SetActive(false);

                  if (levelCount > 1)
                {

                    lastLastPlattform = lastPlattform;
                    if (lastLastPlattform != null)
                    {
                        lastLastPlattform.SetActive(false);
                        lastLast.transform.position = lastLastPlattform.transform.position;
                        lastLast.transform.localEulerAngles = lastLastPlattform.transform.localEulerAngles;
                    }
                }
                if (levelCount > 0)
                {

                    lastPlattform = currentPlattform;
                    if (lastPlattform != null)
                    {
                        lastPlattform.SetActive(false);
                        last.transform.position = lastPlattform.transform.position;
                        last.transform.localEulerAngles = lastPlattform.transform.localEulerAngles;
                    }
                }

                currentPlattform = plattforms[levelCount];
                currentPlattform.SetActive(true);

 
                if (levelCount < plattforms.Count - 1)
                {
                    targetPlattform = plattforms[levelCount + 1];
                    targetPlattform.SetActive(true);
                }
                if (levelCount < plattforms.Count - 2)
                {
                    nextPlattform = plattforms[levelCount + 2];
                    next.transform.position = nextPlattform.transform.position;
                    next.transform.localEulerAngles = nextPlattform.transform.localEulerAngles;
                    next.SetActive(true);
                }
                if (levelCount < plattforms.Count - 3)
                {
                    nextNextPlattform = plattforms[levelCount + 3];
                    nextNext.transform.position = nextNextPlattform.transform.position;
                    nextNext.transform.localEulerAngles = nextNextPlattform.transform.localEulerAngles;
                    nextNext.SetActive(true);
                }

                levelCount += 1;
            }

        }
    }
    public void firstStep()
    {
         if (!tutorial)
        {
            if (setupPlattforms)
            {
                 GameObject[] tempPlattform = GameObject.FindGameObjectsWithTag("level");
                foreach (GameObject go in tempPlattform)
                {
                    plattforms.Add(go);
                    go.SetActive(false);
                }
                plattforms = sortPlatforms(plattforms);
                //plattforms.OrderBy(x => x.name).ToList();
                last = GameObject.Find("LAST");
                lastLast = GameObject.Find("LASTLAST");
                next = GameObject.Find("NEXT");
                nextNext = GameObject.Find("NEXTNEXT");
                setupPlattforms = false;
                GameObject.Find("VRCamera").GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("level");
            }
            
            last.SetActive(false);
            lastLast.SetActive(false);
            next.SetActive(false);
            nextNext.SetActive(false);
        
              if (levelCount > 1)
            {

                lastLastPlattform = lastPlattform;
                if (lastLastPlattform != null)
                {
                    lastLastPlattform.SetActive(false);
                    lastLast.transform.position = lastLastPlattform.transform.position;
                    lastLast.transform.localEulerAngles = lastLastPlattform.transform.localEulerAngles;
                }
            }
            if (levelCount > 0)
            {

                lastPlattform = currentPlattform;
                if (lastPlattform != null)
                {
                    lastPlattform.SetActive(false);
                    last.transform.position = lastPlattform.transform.position;
                    last.transform.localEulerAngles = lastPlattform.transform.localEulerAngles;
                }
            }

            currentPlattform = plattforms[levelCount];
            currentPlattform.SetActive(true);

 
            if (levelCount < plattforms.Count - 1)
            {
                targetPlattform = plattforms[levelCount + 1];
                targetPlattform.SetActive(true);
            }
            if (levelCount < plattforms.Count - 2)
            {
                nextPlattform = plattforms[levelCount + 2];
                next.transform.position = nextPlattform.transform.position;
                next.transform.localEulerAngles = nextPlattform.transform.localEulerAngles;
                next.SetActive(true);
            }
            if (levelCount < plattforms.Count - 3)
            {
                nextNextPlattform = plattforms[levelCount + 3];
                nextNext.transform.position = nextNextPlattform.transform.position;
                nextNext.transform.localEulerAngles = nextNextPlattform.transform.localEulerAngles;
                nextNext.SetActive(true);
            }

            levelCount += 1;
        }
    }
}
