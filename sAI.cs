using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Reflection;
using Rewired.UI.ControlMapper;
using System.Runtime.InteropServices.WindowsRuntime;
using I2.Loc.SimpleJSON;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Assertions.Must;

namespace snakeyAI
{   

    class Node
    {
        //four corners of the "intersection"
        int startx = 0;
        int startz = 0;
        int endx = 0;
        int endz = 0;
        public int nameindex = 0; //FOR DEBUGGING PURPOSES
        List<int> busid = new List<int>();
        int dropoffID = -1;
        //bool isbusActive = false; //always false if bus is false. one if at least one attached bus stop is on
        //bool isdropoffActive = false; //always false if dropoff is false
        int dropOfBetween = -1; //-1 means its not btw nodes. otherwise index of the node in nexts


        public List<Node> nexts = new List<Node>();//the nodes associated with connections
        public List<int> yrot = new List<int>();

        public Node(int sx, int sz, int  ex, int ez, int id,int bs = -1, int drop = -1)
        {
            startx = sx;
            startz = sz;   
            endx = ex;
            endz = ez;
            nameindex = id;
  
            busid.Add(bs);
            dropoffID = drop;
        }

        /*
        public void busstopOn()
        {
            isbusActive = true;
        }
        public void busstopOf()
        {
            isbusActive = false;
        }
        public void dropOn()
        {
            isdropoffActive = true;
        }
        public void dropoff()
        {
            isdropoffActive = false;
            Debug.Log("Dropped off at " + dropoffID.ToString()); //marking where we dropped stuff
        }*/
        public void setdroploc(int nodeind)
        {
            dropOfBetween = nodeind;
        }
        public void addbusstop(int busi)
        {
            busid.Add(busi);
        }
        public int getdroploc()
        {
            return dropOfBetween;
        }
        public void addloc(Node node,int yr)
        {
            nexts.Add(node);
            yrot.Add(yr);
        }
        public bool inside(float x, float z)
        {
            if (startx <= x && x <= endx &&  startz <= z && z <= endz)
            {
                Debug.Log("We hit node " + nameindex.ToString());
                return true;
            }

            return false;
        }
        public string box()
        {
            return "start x " + startx.ToString()+"start z " +startz.ToString() +"end  x " + endx.ToString()+"end z " + endz.ToString(); 
        }
        public int getind() //for debugging know what node I'm on
        {
            return nameindex;
        }


    }
    class gamemap
    {
        public Node current;
        Node activeDrop;
        List<Node> drops;
        //string map = "paris";
        public gamemap(string map)
        {
            if (map.Equals("paris"))
            {
                current = new Node(90, -35, 110, -15, 0,  -1, 3) ; //bridge dropoff
                current.setdroploc(1);//make it 1

                Node n1 = new Node(170, -20, 190, 3,1);//right before th effiel
                current.addloc(n1,79);

                //two deleted since may not be needed (curve?)

                Node n3 = new Node(280, 45, 300, 60, 3, 32,0);//has one bus stop. CHEKC COORDS, slanted  one drop
                n1.addloc(n3,62);
                n1.addloc(current, 79 + 180);


                //has two bus stops 33
                Node n4 = new Node(250, 115, 270, 130, 4, 34);//also slanted by the rivver eiffel
                n4.addbusstop(33);
                n3.addloc(n4,327);
                n3.addloc(n1, 62 + 180);


                //n5 deleted like n2 was

                Node n6 = new Node(75, 30, 105, 60, 6,-1,3);//
                n6.addloc(current, 170);
                n6.addloc(n4,(237+180)%360);
                n6.setdroploc(1);
                n4.addloc(n6,237);
                n4.addloc(n3, (327 + 180) % 360);

                current.addloc(n6,350);

                //node 7

                Node n7 = new Node(55,100,75,116,7,20);//sx sz ez ez
                
                n7.addloc(n6, (340 + 180) % 360);
                n6.addloc(n7, 340);

                //node 8

                Node n8 = new Node(35,170,60,190,8,25);
                n8.addbusstop(17);
                n8.addloc(n7, (345 + 180) % 360);
                n7.addloc(n8, 345);

                Node n9 = new Node(-35, 170, -15, 190, 9, 17); 
                n8.addloc(n9, 273);
                n9.addloc(n8, (273 + 180) % 360);
                n9.addbusstop(22);

                Node n10 = new Node(-20, 85,1,100,10,22);
                n10.addloc(n7, (257 + 180) % 360);
                n7.addloc(n10,257);
                n10.addloc(n9, (170 + 180) % 360);
                n9.addloc(n10,170);
                n10.addbusstop(23);

                Node n11 = new Node(-6,10,9,34, 11, 23); //check small intersection?
                n11.addloc(n6, (253 + 180) % 360);
                n6.addloc(n11, 253);
                n11.addbusstop(31);
                n6.addbusstop(31);
                n10.addloc(n11, 174);
                n11.addloc(n10, (174 + 180) % 360);
                n11.addbusstop(30);

                Node n12 = new Node(-120,10, -105,40, 12, 30); //forward also29

                n12.addloc(n11, (273 + 180)  % 360);
                n11.addloc(n12, 273 );


                Node n13 = new Node(-110,82,-90,100, 13, 27);
   
                n13.addloc(n10, (268 + 180) % 360);
                n10.addloc(n13, 268);
          
                n12.addloc(n13, 9);
                n13.addloc(n12, (9 + 180) % 360);


                Node n14 = new Node(-110,150, -90,180, 14, 18,0);
      
                n13.addloc(n14, 2);
                n14.addloc(n13, (2 + 180) % 360);
                n9.addloc(n14, 262);
                n14.addloc(n9, (262 + 180) % 360);
                n14.setdroploc(2);

                Node n15 = new Node(-190,85,-170,125, 15,28, 0);
                n15.setdroploc(1);

                n13.addloc(n15, 276);
                n15.addloc(n13, (276 + 180) % 360);
                n14.addloc(n15, 230 );
                n15.addloc(n14, (230 + 180) % 360);

                Node n16 = new Node(-190,30, -175,50, 16, 26);
           
                n12.addloc(n16, 280);
                n16.addloc(n12, (280 + 180) % 360);
                n15.addloc(n16, (10 + 180) % 360);
                n16.addloc(n15, 10);

                //stops taken = 33,34,20,24,25,22,23,17,31,30,27,19,18,21,28,32,26,29
                //just 0-16 lfet and subway
                //36-39 ?>???

                //dont think i need bus stops anyways

                Node n17 = new Node(-130,-40,-110,-20,17     );
                n12.addloc(n17, 191);
                n17.addloc(n12, (191 + 180) % 360);


                Node n18 = new Node(-75,-50, -50,-31, 18       );
                n17.addloc(n18, 93);
                n18.addloc(n17, (93 + 180) % 360);

                Node n19 = new Node(0,-45, 16,-20, 19      );
                n19.addloc(current, 76);
                current.addloc(n19, (76 + 180) % 360);
                n18.addloc(n19, 88);
                n19.addloc(n18, (88 + 180) % 360);
                n11.addloc(n19, 170);
                n19.addloc(n11, (170 + 180) % 360);


                Node n20 = new Node(-146,-120, -130,-95, 20    );
                n17.addloc(n20, 191);
                n20.addloc(n17, (191 + 180) % 360);


                Node n21 = new Node(-80,-130,-60,-105, 21    );
                n20.addloc(n21, 97);
                n21.addloc(n20, (97 + 180) % 360);
                n18.addloc(n21, 190);
                n21.addloc(n18, (190 + 180) % 360);

                Node n22 = new Node(10,-130,30,-110, 22, -1  ,5);//this is the dropoff there is nothing else
                n21.addloc(n22, 88);
                n22.addloc(n21, (88 + 180) % 360);


                Node n23 = new Node(95,-115,115,-90, 23     );
                n22.addloc(n23, 79);
                n23.addloc(n22, (79 + 180) % 360);
                current.addloc(n23, 173);
                n23.addloc(current, (173 + 180) % 360);

                Node n24 = new Node(175 ,-110, 200,-80, 24  );
                n23.addloc(n24, 84);
                n24.addloc(n23, (84 + 180) % 360);
                n1.addloc(n24, 171);
                n24.addloc(n1, (171 + 180) % 360);

                Node n25 = new Node(-90, -200, -65, -180, 25);
                n21.addloc(n25, 189);
                n25.addloc(n21, (189 + 180) % 360);


                Node n26 = new Node(10,-195,30,-175, 26);
                n25.addloc(n26, 85);
                n26.addloc(n25, (85 + 180) % 360);
                n22.addloc(n26, 181);
                n26.addloc(n22, (181 + 180) % 360);

                Node n27 = new Node(100, -190, 125, -170, 27);
                n26.addloc(n27, 83);
                n27.addloc(n26, (83 + 180) % 360);
                n12.addloc(n17, 191);
                n17.addloc(n12, (191 + 180) % 360);
                n23.addloc(n27, 179);
                n27.addloc(n23, (179 + 180) % 360);
            }
        }
    }
    //todo check dropoff.ontriggerenter

    /*TODO post patch bus stop activations to keep a good track of which */
    public class sAI :MelonMod
    {
        public static int bonus = -1;
        public static int turning = 0;
        public static int times = 0;
        bool isTurning = false;
        int turnTarget = 0;
        Vector3 pos = Vector3.zero;
        float rotation = 0f;
        gamemap gm;
        Node start;
        Node prev;
        bool AIOn = false;
        float[][] RLarray = new float[28][];
        static float[] expl = new float[28];//float[][] expl = new float[28][];
        Queue<int> previous = new Queue<int>();
        bool training = false;
        System.Random r = new System.Random();
        int nextIndex =0;
        int nodes = 0;
        int[] tracker = new int[28];

        /*current.player.GetAxisRaw("Turn") = D = 1 = TURN RIGHT
         current.player.GetAxisRaw("Turn") = A = -1 = TURN LEFT
        ACCELERATE = 1 = speed up
        ACCELERATE = -1 = SLOW DOWN
        from player.getaxisraw*/

        //turning left is - rotation right is + rotation.
        //rotation is a 360 thing. positive = turn right, negative = turn left
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            gm = new gamemap("paris");
            start = gm.current;
            prev = start;
            previous.Enqueue(19);
            previous.Enqueue(19);
            previous.Enqueue(19);
            RLarray[0] = new float[4];
            RLarray[1] = new float[3];
            RLarray[2] = new float[0];
            RLarray[3] = new float[2];
            RLarray[4] = new float[2];
            RLarray[5] = new float[0];
            RLarray[6] = new float[4];
            RLarray[7] = new float[3];
            RLarray[8] = new float[2];
            RLarray[9] = new float[3];
            RLarray[10] = new float[4];
            RLarray[11] = new float[4];
            RLarray[12] = new float[4];
            RLarray[13] = new float[4];
            RLarray[14] = new float[3];
            RLarray[15] = new float[3];
            RLarray[16] = new float[2];
            RLarray[17] = new float[3];
            RLarray[18] = new float[3];
            RLarray[19] = new float[3];
            RLarray[20] = new float[2];
            RLarray[21] = new float[4];
            RLarray[22] = new float[3];
            RLarray[23] = new float[4];
            RLarray[24] = new float[2];
            RLarray[25] = new float[2];
            RLarray[26] = new float[3];
            RLarray[27] = new float[2];
            resetExpl();

        }
        public override void OnUpdate()
        {
            if ( AIOn)
            {
                if (Input.GetKeyDown(KeyCode.R))
                { //restart the run
                    gm.current = start;
                    prev = start ;
                    bonus = -10;
                    updateRL();
                    times = 0;
                    previous = new Queue<int> ();
                    previous.Enqueue(19);
                    previous.Enqueue(19);
                    previous.Enqueue(19);
                    resetExpl ();
                }
                GameStateHandler current = GameObject.Find("GameState(Clone)").GetComponent<GameStateHandler>();

                if (current.currentState == GameStateNames.GameStateName.Driving)
                {
                    //current.playerBus.drive =playing right now
                    
                    pos = current.playerBus.rb.position;
                    rotation = current.playerBus.rb.rotation.eulerAngles.y;
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("pos " + pos.x.ToString() + "  " + pos.z.ToString());
                        Debug.Log("targets" + gm.current.box());
                    }
                    bool hit = gm.current.inside(pos.x, pos.z);
                    if (hit) //we hit a node, prepare to turn
                    { //step 1: select next destination (set to 0 for now) (run reinforcement otherwise)
                      ////start turning 

                        tracker[gm.current.nameindex] += 1;
                        nodes++;
                        if (nodes == 100)
                        {
                            Debug.Log("WE HIT 100!!");
                        }
                        updateRL();
                        bonus = -1;

                        advance(gm.current.nameindex);
                        prev = gm.current;
                        nextIndex = next(); //this would be where we choose the path, hardcoded to 0 rn
                        turnTarget = gm.current.yrot[nextIndex];

                        //Debug.Log("hit , going to" + nextIndex);


                        turns(rotation, turnTarget);
 

                        gm.current = gm.current.nexts[nextIndex];
                        Debug.Log("hit , going to" + gm.current.nameindex);
                    }
                    if (isTurning)//we are currently nodeswapping
                    {
                        //Debug.Log("Turning  now" + turning.ToString());
                        //Debug.Log("target rotation " + turnTarget.ToString() + "current is " + rotation.ToString());
                        float turnamt = Math.Abs(rotation - turnTarget);
                        if ((turnamt < 4 || 
                            (turning == -1 && (rotation < turnTarget ) ) || 
                            (turning == 1 && (rotation > turnTarget) ))&& turnamt < 180) //close enough or overturned
                        {//stop turning we hit
                            //TODO edge case when going over 360
                            isTurning = false;
                            turning = 0;
                            //Debug.Log(turnTarget+"let to turn"+ turnamt+"right now +"+ rotation);
                        }    
                    }
                    //Debug.Log("Turning  " + rotation + "  time  " + times.ToString());

                    if (current.playerBus.drive.state.isAerial)
                    {
                        //TODO keep counter of how long is in air before boosting
                        //boosting(current); //one wheel maybe fell off so boost 
                    }

                }
            }


            if (Input.GetKeyDown( KeyCode.I))
            {
                //get the current state of the game. handles most important things
                /*
                GameStateHandler current = GameObject.Find("GameState(Clone)").GetComponent<GameStateHandler>();
                rotation = current.playerBus.rb.rotation.eulerAngles.y;
                Debug.Log("Turning  " +rotation + "  time  " + times.ToString());
                */
                AIOn = true;
                Debug.Log("turning on AI");


            }

            if (Input.GetKeyDown(KeyCode.L)) //load
            {
                //saveRL();
                for (int i = 0; i < 28; i++)
                {
                    Debug.Log($"NODE {i}  is been hit {tracker[i]}");
                }
            }
       
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                printrl();
                Debug.Log("times = " + times);
            }
            if ((Input.GetKeyDown(KeyCode.N)))
            {
                Debug.Log("We've hit nodes " + nodes);
            }

            if ((Input.GetKeyDown(KeyCode.J)))
            {
                loadRL();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                printexp();
            }
  

            if (Input.GetKeyDown(KeyCode.M)) //Fast exit 
            {
                Application.Quit();
            }
        }

        private int next()
        {
            //return 0;

            //todo beeline to goal if full. 

            /*if (training)
            {
                int explorechance = r.Next(0, 9);
                if (explorechance < 3) //randomly explore the world  at a 0.4 chance
                {
                    Debug.Log("Training");
                    int ne = r.Next(0, RLarray[gm.current.nameindex].Length);
                    while (previous.Contains(gm.current.nexts[ne].nameindex))
                    {
                        ne = r.Next(0, RLarray[gm.current.nameindex].Length);
                    }
                    updateExpl(gm.current.nameindex, ne);

                    return ne;
                }

            }*/
            float[] sortedA = (float[]) RLarray[gm.current.nameindex].Clone();
            for (int i = 0; i < sortedA.Length; i++)
            {
                sortedA[i] += expl[gm.current.nexts[i].nameindex];
            }
            float[] ind = (float[])sortedA.Clone();
            Array.Sort(sortedA);
            Array.Reverse(sortedA);
            Debug.Log("No Training");        
            foreach (float a in sortedA)
            {
                int n = Array.IndexOf(ind, a);
                Debug.Log("n is" + n + "and a is " + a + "originally" + RLarray[gm.current.nameindex][n] + "minusing" + expl[gm.current.nexts[n].nameindex]);
                if (!(previous.Contains(gm.current.nexts[n].nameindex)))
                {
                    //Debug.Log("Not in previous!");
                    updateExpl(gm.current.nexts[n].nameindex, n);

                    return n;
                }
            }
          

            Debug.Log("JUST DEFAULT?? SOMTHING IS WRONG");
            updateExpl(gm.current.nexts[0].nameindex, 0);

            return 0;
              
 
        }

        private void updateRL()
        {
            //0.4 = learning rate
            //discount of 0.95
            RLarray[prev.nameindex][nextIndex] += 0.4f * (bonus + 0.95f* RLarray[gm.current.nameindex].Max() - RLarray[prev.nameindex][nextIndex]);
        }

        private void advance(int n)

        {
            previous.Enqueue(n);
            previous.Dequeue();
        }

        private void printrl()
        {
            for (int i = 0; i < RLarray.Length; i++)
            {
                Debug.Log("row " + i);
                for(int j = 0; j < RLarray[i].Length; j++)
                {
                    Debug.Log(RLarray[i][j]+"\t");
                }
                Debug.Log("");    
            }
        }

        private void printexp()
        {
            for (int i = 0; i < expl.Length; i++)
            {
    
                    Debug.Log("NODE  " +i +  "   " +expl[i] + "\t");
                
                //Debug.Log("");
            }
        }

        private void saveRL()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, RLarray);

            stream.Position = 0; // Reset the stream position.
            formatter.Serialize(File.OpenWrite(@"RLarray.bin"), RLarray);
        }

        private void updateExpl(int row, int col)
        {
            float mult = 1f;
            if (times > 2)
            {
                mult = times * 0.5f;
            }
            expl[row] -= 2.5f * mult;
        }
        public static void resetExpl()
        {
            expl = new float[28];
            /*AAAAAA
            expl[0] = new float[4];
            expl[1] = new float[3];
            expl[2] = new float[0];
            expl[3] = new float[2];
            expl[4] = new float[2];
            expl[5] = new float[0];
            expl[6] = new float[4];
            expl[7] = new float[3];
            expl[8] = new float[2];
            expl[9] = new float[3];
            expl[10] = new float[4];
            expl[11] = new float[4];
            expl[12] = new float[4];
            expl[13] = new float[4];
            expl[14] = new float[3];
            expl[15] = new float[3];
            expl[16] = new float[2];
            expl[17] = new float[3];
            expl[18] = new float[3];
            expl[19] = new float[3];
            expl[20] = new float[2];
            expl[21] = new float[4];
            expl[22] = new float[3];
            expl[23] = new float[4];
            expl[24] = new float[2];
            expl[25] = new float[2];
            expl[26] = new float[3];
            expl[27] = new float[2];*/
        }
        private void loadRL()
        {
            RLarray = (float[][])new BinaryFormatter().Deserialize(File.OpenRead(@"RLarray.bin"));

        }
        
        //REMEMBER TO CHECK DRIVE STATE = DRIVE1
       private void turnRight()
        {
            turning = 1;
        }

        private void turnLeft()
        {
            turning = -1;
        }

        private void stopTurning()
        {
            turning = 0;
        }

        private void turns(float source, int dest)//start the turbning
        {
            float t = closestdist(source, dest);
            if (t > 0)
            {
                isTurning = true;
                Debug.Log("we will turn right!!");

                turnRight();

            }
            else if (t < 0)
            {
                isTurning = true;
                Debug.Log("we will turn left!!");

                turnLeft();
            }
            else
            {
                Debug.Log("No turning i guess");
            }

        }

        private float closestdist(float source, int dest)
        {
            if (Math.Abs(dest-source) > 180)
            {
                return ((source + 180) % 360 - dest);
            }
            else
            {
                return dest - source;
            }
        }



        private void boosting(GameStateHandler current)
        {
            if (current.currentState == GameStateNames.GameStateName.Driving)
            {
                //current.playerBus.boost.isInfiniteBoost = true;
                current.playerBus.boost.airBoostInput = 1f;//should be boosting

                Type typeofboost = typeof(NetworkBoost);
                FieldInfo fi = typeofboost.GetField("isBoosting", BindingFlags.NonPublic | BindingFlags.Instance);
                fi.SetValue(current.playerBus.boost, true);
            }
       }


    }
    [HarmonyPatch(typeof(NetworkDriveState), "usePlayerInput")]
    class TurnPatch
    {
        static void Postfix(NetworkDriveState __instance)
        {
            if (sAI.turning == 1)
            {
                __instance.currentSteering = 90f;
            }
            else if (sAI.turning == -1)
            {
                __instance.currentSteering = -90f;
            }
            //no else should automatically go back to zero because no player input 
        }
    }

    [HarmonyPatch(typeof(Bus), "foundBusStop")]
    class BusstopPatch
    {
        static void Prefix(Bus __instance) {
            
            
            if (__instance.passengerCount < __instance.maxPassengerCapacity())
            {
                Debug.Log("Bus Bonus");
                sAI.bonus += 1;
            }
        }
    }

    [HarmonyPatch(typeof(TransitBus), "foundDropOff")]
    class DropPatch
    {
        static void Prefix(Bus __instance)
        {
            Debug.Log("Found drop off");
            sAI.times++;
            sAI.bonus += 1;
            sAI.resetExpl();
            if (__instance.passengerCount > 0)
            {
                sAI.bonus += 5 * (__instance.passengerCount / __instance.maxPassengerCapacity());                                                                                                                                                                                           
            }
            //no else should automatically go back to zero because no player input 
        }
    }



    //BUSSTOP foundBusStop patch

    //Drop Off foundDropOff()

    //POI TARGET/TRACKER
    //Vector3 vector3 = this.target.transform.position;
    //from POI target 
    //get to dropoffs go parisonly/gameonly ->gameplay -> dropoff

    //also for bus stops
    //IF NOT GROUNDEd (networkDriveState.checkGrounded/isAriel) USE BOOST)

    // bus components, gamestatehandler, networkdrive, dropoffmanager #important
    //bus.arrow for the gps arrow
    //playerbus(steering wheel) from  gamestate to move

    //unit Bus
    //METHOD foundDropOff() mark 
    //METHODD foundBusStop(), check there is space
    //passengercount

    //busDie from busComponents = stop AI
    //busStats

    //busstopmanager -> BusStopZone[] busStops

    //cARAI - find nearest node

    //dropoofmanager
    //gpsarrow this.target

    //check busselectAI.

    /*
    [HarmonyPatch(nameof(GameModeForm), "Start")]
    public static class Patch
    {
        private static void Prefix(GameModeForm __instance)
        {
            //GameModeOption AIopt = new GameModeOption();
            //AIopt.gameModeLabel.text = "AI";

            #//var gamemodes = __instance.FieldGetter("GameModeOption[]","options")
            /*
            Type gamemodes = typeof(GameModeForm);
            FieldInfo gmlist = gamemodes.GetField("options", BindingFlags.NonPublic | BindingFlags.Instance);

            GameModeOption[] gms = gmlist.GetValue(__instance) as GameModeOption[];
            gms.AddItem(AIopt);

            GameModeOption[] gg = { AIopt};
            gmlist.SetValue(__instance, gg);
            __instance.name = gms.Length.ToString();/
        }

    }*/


}
