using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameControl : MonoBehaviour {


    public GUISkin Skin;

    Camera MainCam;
    [SerializeField]
    GameObject MainCameraObj;
    [SerializeField]

    private Material Team1Mat;
    [SerializeField]
    private Material Team2Mat;
    [SerializeField]
    private Material cellMaterialInactive;
    public GameObject weirdObject;

    public Text Team1Moves;
    public Text Team2Moves;

    public Text Team1FixedLeft;
    public Text Team2FixedLeft;

    public Text Team1Distance;
    public Text Team2Distance;

    public SimpleGrid[] datGrid = new SimpleGrid[1];

    private float cellSize;

    private string T1 = "T1";
    private string T2 = "T2";

    private Vector3Int _gridSize = new Vector3Int(10, 10, 10);

    private Color randColor1;
    private Color randColor2;
    private Color transColor;

    int team1Count;
    int team2Count;

    int T1Dist;
    int T2Dist;

    int T1TargetsRemaining;
    int T2TargetsRemaining;

    // Use this for initialization
    void Start()
    {
        cellSize = 1f;
        
        randColor1 = UnityEngine.Random.ColorHSV();
        randColor2 = UnityEngine.Random.ColorHSV();

        transColor = Color.clear;
        
       datGrid[0] = new SimpleGrid(weirdObject, _gridSize, cellSize, randColor1, randColor2, transColor, cellMaterialInactive,  T1, T2);

       StartCoroutine(datGrid[0].GamePlay(Team1Mat, Team2Mat));

    }

//Initializing 'team win' to false for both
    public bool _Team1Wins = false;
    public bool _Team2Wins = false;

    void Update () {

        //Game Stats
        T1TargetsRemaining = datGrid[0].getT1TargetRemaining();
        T2TargetsRemaining = datGrid[0].getT2TargetRemaining();

        //Check if a team has won
        _Team1Wins = Team1Wins();
        _Team2Wins = Team2Wins();

        //If so, end game
        if (_Team1Wins == true || _Team2Wins == true)
            Time.timeScale = 0;

        //Update gameStats;
        team1Count =  datGrid[0].getT1Count();
        team2Count = datGrid[0].getT2Count();
        T1Dist = datGrid[0].getDistT1();
        T2Dist = datGrid[0].getDistT2();
        Team2MoveCount();
        Team1MoveCount();
        Team1FixedRemaining();
        Team2FixedRemaining();
        Team1Dist();
        Team2Dist();
    }

    //Functions to determine if a team has won
    bool Team1Wins()
    {
        if (T1TargetsRemaining < 1) return true;
        else return false;
    }
    bool Team2Wins()
    {
        if (T2TargetsRemaining < 1) return true;
        else return false;
    }

    //GUI visualization
    private void OnGUI()
    {
        var myStyle = new GUIStyle();
        myStyle.fontSize = 80;
        myStyle.normal.textColor = Color.red;

        GUI.skin = Skin;

        if (_Team1Wins == true)
        {
            GUI.Label(new Rect(1400, 1080, 200, 500), "TEAM 1 WINS WITH " + "\n" + team1Count + " MOVES", myStyle);
        }

        if (_Team2Wins == true)
        {
            GUI.Label(new Rect(1400, 1080, 200, 500), "TEAM 2 WINS WITH " + "\n" + team2Count + " MOVES", myStyle);
        }
    }

    //Game stat functions
    void Team1FixedRemaining()
    {
        Team1FixedLeft.text = ("T1Fixed Left: " + T1TargetsRemaining).ToString();
    }

    void Team2FixedRemaining()
    {
        Team2FixedLeft.text = ("T2Fixed Left: " + T2TargetsRemaining).ToString();
    }

    void Team2MoveCount()
    {
        Team2Moves.text = ("T2 Moves: " + team2Count);
    }

    void Team1MoveCount()
    {
        Team1Moves.text = ("T1 Moves: " + team1Count);
    }

    void Team1Dist()
    {
        Team1Distance.text = ("T1 Cumulative Distance Remaining " + T1Dist);
    }

    void Team2Dist()
    {
        Team2Distance.text = ("T2 Cumulative Distance Remaining " + T2Dist);
    }



}
