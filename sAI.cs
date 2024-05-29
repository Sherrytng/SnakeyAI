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

        public Node(int sx, int sz, int  ex, int ez, int id, int yr = 0,int bs = -1, int drop = -1)
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

                Node n3 = new Node(280, 45, 300, 60, 3, 32,1);//has one bus stop. CHEKC COORDS, slanted  one drop
                n1.addloc(n3,62);
                n1.addloc(current, 79 + 180);


                //has two bus stops 33
                Node n4 = new Node(250, 115, 270, 130, 4, 34);//also slanted by the rivver eiffel
                n4.addbusstop(33);
                n3.addloc(n4,326);
                n3.addloc(n1, 62 + 180);


                //n5 deleted like n2 was

                Node n6 = new Node(75, 30, 105, 60, 6,-1,3);//
                n6.addloc(current, 170);
                n6.addloc(n4,(237+180)%360);
                n6.setdroploc(1);
                n4.addloc(n6,237);
                n4.addloc(n3, (326 + 180) % 360);

                current.addloc(n6,350);

            }
        }
    }
    //todo check dropoff.ontriggerenter

    /*TODO post patch bus stop activations to keep a good track of which */
    public class sAI :MelonMod
    {
        int times = 0;
        public static int turning = 0;
        bool isTurning = false;
        int turnTarget = 0;
        Vector3 pos = Vector3.zero;
        float rotation = 0f;
        gamemap gm;
        Node start;
        bool AIOn = false;
        int turnovercircle = 0;

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
        }
        public override void OnUpdate()
        {
            if ( AIOn)
            {
                if (Input.GetKeyDown(KeyCode.R))
                { //restart the run
                    gm.current = start;
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
                        Debug.Log("hit , going to" + gm.current.nexts[0].nameindex.ToString());

                        int nextIndex = 0; //this would be where we choose the path, hardcoded to 0 rn
                        turnTarget = gm.current.yrot[nextIndex];

                        

                        if (turnTarget < rotation || (Math.Abs((turnTarget + 180) % 360 - rotation) < 180))
                        {
                            Debug.Log("we will turn left");
                            if ((Math.Abs((turnTarget + 180) % 360 - rotation) < 180))
                            {
                                Debug.Log("Turning over 360");
                                turnovercircle = 1;
                            }
                            turnLeft(); //need to subtract so left
                        }
  
                        else
                        {
                            Debug.Log("We will turn right");
                            if ((Math.Abs((turnTarget + 180) % 360 - rotation) > 180))
                            {
                                Debug.Log("Turning over 360");
                                turnovercircle = 1;
                            }
                            turnRight();
                        }
                        gm.current = gm.current.nexts[nextIndex];
                        isTurning = true;                  
                    }
                    if (isTurning)//we are currently nodeswapping
                    {
                        Debug.Log("Turning  now" + turning.ToString());
                        Debug.Log("target rotation " + turnTarget.ToString() + "current is " + rotation.ToString());
                        if (Math.Abs(rotation - turnTarget) < 4 || 
                            (turning == -1 && (rotation < turnTarget  ))|| 
                            (turning == 1 && (rotation > turnTarget || (turnovercircle ==1 && rotation > turnTarget)))) //close enough or overturned
                        {//stop turning we hit
                            //TODO edge case when going over 360
                            isTurning = false;
                            turning = 0;
                            turnovercircle = 0;
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



            //testing if they work
            if (Input.GetKeyDown(KeyCode.K))
            {
                turnRight();
                times=0;
                Debug.Log("Turning Right"+ times.ToString());

            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                turnLeft();
                times=0;
                Debug.Log(" turning left "+ times.ToString());
            }
  

            if (Input.GetKeyDown(KeyCode.M)) //Fast exit 
            {
                Application.Quit();
            }
        }

        
        //REMEMBER TO CHECK DRIVE STATE = DRIVE
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
    class Patch
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
