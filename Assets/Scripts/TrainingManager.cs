using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
// using System;
using System.Reflection;

public class TrainingManager : MonoBehaviour
{   
    string host = "127.0.0.1"; //; "localhost"
    public int port = 5060;
    Socket client;

    const int messageLength = 10000; //12000
    byte[] messageHolder = new byte[messageLength];
    readonly ConcurrentQueue<string> inMessage = new ConcurrentQueue<string>();

    Thread t;

    public Robot robot;


    [SerializeField]
    GameObject target;

    [SerializeField]
    GameObject trailTarget;

    [SerializeField]
    GameObject trailClosest;

    [SerializeField]
    GameObject trailSecond;

    [SerializeField]
    GameObject obstacle1;

    [SerializeField]
    GameObject obstacle2;

    // [SerializeField]
    // GameObject c0;

    // [SerializeField]
    // GameObject c1;

    // [SerializeField]
    // float logProb = 0.01f;

    enum Phase {
        Freeze,
        Run
    } 
    Phase phase;
    public float stepTime = 0.05f; //0.1f
    public float currentStepTime = 0.0f;

    Vector3 newTarget;
    // Dictionary<string, Dictionary<string, float>> state = new Dictionary<string, Dictionary<string, float>>();
    // public Dictionary<string, float> minRange = new Dictionary<string, float>();

    public BezierCurve curver;

    public System.Random random = new System.Random();

    Transform base_footprint;

    // List<float> radius = new List<float>{4f, 4.5f, 5f};
    float radius = 4.5f;
        
    void Awake()
    {
        base_footprint = robot.transform.Find("base_link");
    }

    void Start()
    {   
        // Robot.State state = robot.GetState();
        // originPos = new Vector3(state.baseLinkPos.x, 0.0f, state.baseLinkPos.y);
        
        // client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // client.Connect(IPAddress.Parse(host), port);
        // Debug.Log("Start listening...");
        StartCoroutine(ConnectToServer());

        // float target_x = Random.Range(-3.0f, 3.0f);
        // if (target_x <= 1 && target_x >= -1) {
        //     if (target_x > 0) {
        //         target_x += 1;
        //     } else {
        //         target_x -= 1;
        //     }
            
        // }

        // float target_y = Random.Range(-3.0f, 3.0f);
        // if (target_y <= 1 && target_y >= -1) {
        //     if (target_y > 0) {
        //         target_y += 1;
        //     } else {
        //         target_y -= 1;
        //     }
            
        // }
    
        // newTarget = new Vector3(target_x, 0, target_y);

        // int theta = Random.Range(0, 359);
        // float radius = Random.Range(3.5f, 1.5f);
        // newTarget = new Vector3(Mathf.Cos(theta) * radius, 0, Mathf.Sin(theta) * radius);

        MoveGameObject(target, newTarget);
        // Vector3 originalPos = new Vector3(0,0,0);
        // MoveRobot(originalPos);
        
        ////////straight////////
        // curver.drawLine(originalPos, targetPos);

        ////////curve////////
        // List<Vector3> peak = findPeak(originalPos, newTarget);
        // curver.drawCurve(originalPos, peak[0], peak[1] ,newTarget);
        // MoveGameObject(c0, peak[0]);
        // MoveGameObject(c1, peak[1]);

        // SetObstables(curver);
        State state = updateState(newTarget, curver);
        

        Send(state);
        // Send("state", robot.GetState());

        // EndStep();
    }

     // Update is called once per frame
    void Update()
    {   
        ThreadReceive();

            while (inMessage.TryDequeue(out string jm)) {
                try { 
                    Receive(jm);
                    // Debug.Log(jm);
                }
                catch (System.Exception e){
                    Debug.LogException(e, this);
                    Debug.Log(jm);
                }
                // Debug.Log(ctr);             
            }
        
    }

    void FixedUpdate()
    {

        if (phase == Phase.Run)
            currentStepTime += Time.fixedDeltaTime;
        if (phase == Phase.Run && currentStepTime >= stepTime)
        {
            EndStep();
        }
    }

    void ThreadReceive()
    {   
        if (t != null && t.IsAlive == true)
            return;
        ThreadStart ts = new ThreadStart(StartReceive);
        t = new Thread(ts);
        t.Start();

    }

    void StartReceive()
    {   
        try {
            int bufferLen = client.Receive(messageHolder);
            string jMessage = Encoding.ASCII.GetString(messageHolder, 0, bufferLen);
            inMessage.Enqueue(jMessage);
        } catch {

        }
        
    }

    void StartStep()
    {   
        phase = Phase.Run;
        currentStepTime = 0;
        Time.timeScale = 1;
    }

    void EndStep()
    {
        phase = Phase.Freeze;
        // Time.timeScale = 0; //stop physics simulation
        // Debug.Log(ctr); 
        

        State state = updateState(newTarget, curver);

        Send(state);
                
        // Send("state", robot.GetState());

    }

    void Receive(string jMessage)
    {   
        try {
            JObject jOmessage = JObject.Parse(jMessage);
            // Debug.Log($"recv:\n {jMessage}");

            // if (Random.Range(0, 1f) < logProb)
            //     Debug.Log($"recv:\n {jMessage}");
            
            var title = (string) jOmessage["title"];
            var content = jOmessage["content"];

            switch (title) {
                case "action":
                    var action = content.ToObject<Robot.Action>();
                    // Debug.Log($"action:\n ({action.voltage[0]}, {action.voltage[1]}, {action.voltage[2]}, {action.voltage[3]}, {action.voltage[4]} {action.voltage[5]}, {action.voltage[6]}, {action.voltage[7]})" );
                    
                    robot.DoAction(action);

                    StartStep();

                    break;
                
                case "new target":
                    // robot.trailRenderer.Clear();

                    float target_x = Random.Range(-3.0f, 3.0f);
                        if (target_x <= 1 && target_x >= -1) {
                            if (target_x > 0) {
                                target_x += 1;
                            } else {
                                target_x -= 1;
                            }
                            
                        }
                    float target_y = Random.Range(-3.0f, 3.0f);
                    if (target_y <= 1 && target_y >= -1) {
                        if (target_y > 0) {
                            target_y += 1;
                        } else {
                            target_y -= 1;
                        }   
                    }

                    // int theta = Random.Range(0, 359);
                    // float radius = Random.Range(3.5f, 1.5f);
                    // Vector3 tmp = new Vector3(Mathf.Cos(theta) * radius, 0, Mathf.Sin(theta) * radius);
                    // float target_x = tmp.x;
                    // float target_y = tmp.z;

                    Transform baselink = robot.transform.Find("base_link");
                    // Transform baselink = robot.transform.Find("base_link");
                    var carPos = baselink.GetComponent<ArticulationBody>().transform.position;
                    // var rotation = baselink.GetComponent<ArticulationBody>().transform.rotation;
                    // if (rotation.w < 0 || carPos[1] < -0.39) { //upside down or stucked in floor
                    //     baselink.GetComponent<ArticulationBody>().TeleportRoot(new Vector3(0,0,0), Quaternion.identity);
                    //     carPos = new Vector3(0,0,0);
                    // }

                    newTarget = new Vector3(carPos[0]+target_x, 0, carPos[2]+target_y);
                    Debug.Log("-----------------------");
                    Debug.Log("newTarget: "+ newTarget);
                    MoveGameObject(target, newTarget);

                    ////////straight////////
                    // curver.drawLine(carPos, targetPos);

                    ////////curve////////
                    // List<Vector3> peak = findPeak(carPos, newTarget);
                    // curver.drawCurve(carPos, peak[0], peak[1] ,newTarget);
                    // MoveGameObject(c0, peak[0]);
                    // MoveGameObject(c1, peak[1]);

                    // SetObstables(curver);
                    State state = updateState(newTarget, curver);

                    Send(state);
                    
                    break;
            }
        } catch (System.Exception e) {
            Debug.LogException(e, this);
            Debug.Log($"recv:\n {jMessage}");
        }
        
    }

    private float randomFloat(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    } 

    private List<Vector3> findPeak(Vector3 start, Vector3 end) {
        Vector3 mid = (start + end)/2;
        float minX = Mathf.Min(start[0], end[0]);
        float maxX = Mathf.Max(start[0], end[0]);
        float minY = Mathf.Min(start[2], end[2]);
        float maxY = Mathf.Max(start[2], end[2]);

        // float minPortion0 = randomFloat(0.2f, 0.8f);
        // float minPortion1 = randomFloat(0.2f, 0.8f);
        // while (Mathf.Abs(minPortion1 - minPortion0) <= 0.3) {
        //     minPortion1 = randomFloat(0.2f, 0.8f);
        // }
        // Vector3 c0 = new Vector3(minX + minPortion0 * (maxX - minX), start[1], randomFloat(minY, minY*1.3f));
        // Vector3 c1 = new Vector3(minX + minPortion1 * (maxX - minX), start[1], randomFloat(maxY, maxY*1.3f));
        
        //https://cubic-bezier.com/#.17,.67,.83,.67
        Vector3 c0 = new Vector3(randomFloat(minX, maxX), start[1], randomFloat(minY, minY + Mathf.Abs(maxY - minY)*0.7f));
        Vector3 c1 = new Vector3(randomFloat(minX, maxX), start[1], randomFloat(maxY - Mathf.Abs(maxY - minY)*0.7f, maxY));

        List<Vector3> result = new List<Vector3>{c0, c1};
        return result;
    }

    void Send(object data)
    {   
        // recurring properties in your object, this tells your serializer that multiple references are okay.
        var settings = new Newtonsoft.Json.JsonSerializerSettings();
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        byte[] buffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data, settings));
        client.Send(buffer);

        // Debug.Log($"send:\n {data}");
        // if (Random.Range(0, 1f) < logProb)
        //     Debug.Log($"send:\n {buffer}");
    
    }

    private  IEnumerator PauseCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        // Code after 1 second pause
    }

    private IEnumerator ConnectToServer()
    {
        // Create a TcpClient object and attempt to connect to the server
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        while (!client.Connected)
        {
            try
            {
                client.Connect(IPAddress.Parse(host), port);
        
            }
            catch (SocketException e)
            {   
                Debug.Log(e.ToString());
                // If connection is refused, wait for a short time before trying again
                StartCoroutine(PauseCoroutine());
            }

        }

        // Once the loop has exited, the client is connected to the server
        Debug.Log("Client connected to server");
        yield return null;

    }

    void MoveGameObject(GameObject obj, Vector3 pos)
    {
        obj.transform.position = pos;
    }

    void MoveRobot(Vector3 pos)
    {   
        // Transform baselink = robot.transform.Find("base_footprint");
        // Transform baselink = robot.transform.Find("base_link");
        base_footprint.GetComponent<ArticulationBody>().TeleportRoot(pos, Quaternion.identity);
    }

    State UpdatePath(BezierCurve curver, State state)
    {
        // Vector2 p0 = new Vector2(curver.positions[0].x, curver.positions[0].z);
        // state["short time target pos"]["closest point x"] = p0[0];
        // state["short time target pos"]["closest point y"] = p0[1];

        // Vector2 p1 = new Vector2(curver.positions[2].x, curver.positions[2].z);
        // state["short time target pos"]["second closest point x"] = p1[0];
        // state["short time target pos"]["second closest point y"] = p1[1];

        // Vector2 p2 = new Vector2(curver.positions[12].x, curver.positions[12].z);
        // state["short time target pos"]["farthest point x"] = p2[0]; //10 points distance
        // state["short time target pos"]["farthest point y"] = p2[1];

        // trailClosest.transform.position = new Vector3(p0[0], 0.1f, p0[1]);
        // trailSecond.transform.position = new Vector3(p1[0], 0.1f, p1[1]);
        // trailTarget.transform.position = new Vector3(p2[0], 0.1f, p2[1]);

        float minDist = 100000.0f;
        int closestIndex = 0;
        for (int i = 0; i < curver.numPoints-1; i++) {
            float dist = Vector2.Distance(new Vector2(state.carPosition.x, state.carPosition.z), new Vector2(curver.positions[i].x, curver.positions[i].z));
            if (dist < minDist) {
                minDist = dist;
                closestIndex = i;
            }
        }
        
        Vector3 p0 = new Vector3(curver.positions[closestIndex].x, curver.positions[closestIndex].y, curver.positions[closestIndex].z);
        
        Vector3 p1;
        if ((closestIndex+1) > (curver.numPoints-1)) { //|| (curver.positions[closestIndex+1].x <= 0.001 && curver.positions[closestIndex+1].z <= 0.001)
            p1 = new Vector3(curver.positions[curver.numPoints-1].x, curver.positions[curver.numPoints-1].y, curver.positions[curver.numPoints-1].z);
        } else {
            p1 = new Vector3(curver.positions[closestIndex+1].x, curver.positions[closestIndex+1].y, curver.positions[closestIndex+1].z);
        }
        
        Vector3 p2;
        int offset = 40;
        if ((closestIndex+offset) > (curver.numPoints-1) ) { //|| (curver.positions[closestIndex+offset].x <= 0.001 && curver.positions[closestIndex+offset].z <= 0.001)
            p2 = new Vector3(curver.positions[curver.numPoints-1].x, curver.positions[curver.numPoints-1].y, curver.positions[curver.numPoints-1].z);
        } else {
            p2 = new Vector3(curver.positions[closestIndex+offset].x, curver.positions[closestIndex+offset].y, curver.positions[closestIndex+offset].z);
            
        }
        
        state = robot.UpdatePath(state, p0, p1, p2);

        trailClosest.transform.position = p0;
        trailSecond.transform.position = p1;
        trailTarget.transform.position = p2;

        return state;
    }

    void SetObstables(BezierCurve curver) 
    {
        int curve_idx = random.Next(40, curver.numPoints-40);
        Vector3 pos1 = new Vector3(curver.positions[curve_idx].x + Random.Range(0.1f, 0.2f), curver.positions[curve_idx].y, curver.positions[curve_idx].z + Random.Range(0.1f, 0.2f));
        MoveGameObject(obstacle1, pos1);

        // curve_idx = random.Next(40, curver.numPoints-40);
        // pos1 = new Vector3(curver.positions[curve_idx].x - Random.Range(0.3f, 0.4f), curver.positions[curve_idx].y, curver.positions[curve_idx].z - Random.Range(0.3f, 0.4f));
        // MoveGameObject(obstacle2, pos1);

    }

    State updateState(Vector3 newTarget, BezierCurve curver) 
    {

        State state = robot.GetState(newTarget);
        System.Type type = state.GetType();
        // FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        

        state = UpdatePath(curver, state);

        

        return state;
    }



}
