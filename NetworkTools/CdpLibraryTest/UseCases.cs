using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Marler.NetworkTools
{
    public static class WriterExtensions
    {
        public static void WriteLine(this TextWriter writer, Int32 tabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.WriteLine(fmt, obj);
        }
        public static void WriteLine(this TextWriter writer, Int32 tabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.WriteLine(str);
        }

        public static void Write(this TextWriter writer, Int32 tabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.Write(fmt, obj);
        }
        public static void Write(this TextWriter writer, Int32 tabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.Write(str);
        }
    }

    /*
            WaitingForUserToConnect
            HaveControlWaitingForUser
            HaveControlNoPayloadsSent
            HaveControlWaitingForAck
            Resending
            ConnectedAndHasControl,
            WaitingForAck,
            Resending,
            EnumSize,
      
      
      
            State: <Actor>.<ActorState>
     
            State: User.WaitingForUserDecision
                   CdpClient.WaitingForUserDecision
            SendHeartbeat             > State does not change
            SendHalt                  > User.ConnectionClosed, CdpClient.ConnectionClosed
            SendRandomPayload         > State does not change
            SendNoAck                 > If no resends: State does not change
                                      > Else         : State goes to Resending (if received a resend)
            SendWaitForAck            > State goes to wait for ack
                                      > State goes to Resending (if received a resend)
            SendAndGiveControlNoAck   > State goes to handler
                                      > State goes to resending (if received a resend)
            SendAndGiveControlWithAck > State goes to handler
                                      > State goes to waiting for ack
                                      > State goes to resending (if received a resend)
            
     
            State: CdpController.Resending 
     * 
     * 
            State:       
     *

    */


    static class DecisionExtensions
    {

        public static String ListToString(this ControllerDatagram[] list)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(list[i]);
            }
            return builder.ToString();
        }

    }


    enum ControllerDatagram
    {
        Halt,
        RandomPayload,
        PayloadNoAck,
        PayloadWithAck,
        PayloadAndGiveControlNoAck,
        PayloadAndGiveControlWithAck,
        PayloadAndClose,
        EnumSize,
    }
    enum HandlerDatagram
    {
        Halt,
        Close,
        Ack,
        Resend,
        EnumSize,
    }


    enum ControllerState
    {
        NotConnected,
        ConnectedAndHasControl,
        WaitingForAck,
        Resending,
        EnumSize,


    }

    enum HandlerState
    {
        ListeningForConnections,
        EnumSize,
    }



    /*
    [TestClass]
    public class UseCases
    {
        // Keeps track of which decisions have been made
        class DecisionSet
        {
            Int32[] levelDecisionMadeAt;
            public DecisionSet()
            {
                this.levelDecisionMadeAt = new Int32[(Int32)ControllerState.EnumSize * (Int32)ControllerDatagram.EnumSize];
            }

            // returns -1 if decision was not made yet
            public Int32 CheckDecision(ControllerState state, ControllerDatagram datagram, Int32 levelStartingAtZero)
            {
                Int32 decisionIndex = (Int32)state * (Int32)ControllerDatagram.EnumSize + (Int32)datagram;
                Int32 levelDecisionMadeAt = levelDecisionMadeAt[decisionIndex];

                if(levelDecisionMadeAt > 0) return levelDecisionMadeAt;

                levelDecisionMadeAt[decisionIndex] = levelStartingAtZero + 1; // can't be 0
                return -1;
            }
        }
        // Get an array of all possible datagrams based on a state
        class StateToDatagramDecisionLookup
        {
            //ControllerDatagram[][] decisionTree = new ControllerDatagram[(Int32)ControllerState.EnumSize][];
            ControllerState[][] decisionTree = new ControllerState[(Int32)ControllerState.EnumSize][];
            public StateToDatagramDecisionLookup()
            {

                ControllerState[] aboutToConnectDecisions = new ControllerState[(Int32)ControllerDatagram.EnumSize];
                aboutToConnectDecisions[(Int32)ControllerDatagram.Halt] = ControllerState.NotConnected;
                //aboutToConnectDecisions[(Int32)ControllerDatagram.RandomPayload] = ControllerState.;

                decisionTree[(Int32)ControllerState.NotConnected] = aboutToConnectDecisions;
                
                //decisionTree[(Int32)ControllerState.NotConnected] = new ControllerState[(Int32)ControllerDatagram.EnumSize][];



                decisionTree[(Int32)ControllerState.NotConnected] = new ControllerDatagram[]{
                    ControllerDatagram.Halt,
                    ControllerDatagram.RandomPayload,
                    ControllerDatagram.PayloadNoAck,
                    ControllerDatagram.PayloadWithAck,
                    ControllerDatagram.PayloadAndGiveControlNoAck,
                    ControllerDatagram.PayloadAndGiveControlWithAck,
                    ControllerDatagram.PayloadAndClose
                };
            }
            public ControllerDatagram[] DecisionList(ControllerState state)
            {
                return decisionTree[(Int32)state];
            }
        }


        [TestMethod]
        public void TestMethod1()
        {
            ControllerState controllerState = ControllerState.NotConnected;
            HandlerState handlerState = HandlerState.ListeningForConnections;




            EnumerateDecisions(0, ControllerState.NotConnected, new DecisionSet(), new StateToDatagramDecisionLookup());


        }



        void EnumerateDecisions(Int32 level, ControllerState state, DecisionSet decisionSet, StateToDatagramDecisionLookup lookup)
        {

            ControllerDatagram[] datagramDecisionList = lookup.DecisionList(state);
            if (datagramDecisionList == null || datagramDecisionList.Length <= 0)
            {
                Console.WriteLine(level, "Level {0} State {1}: 0 Decisions", level, state);
            }
            else
            {
                Console.WriteLine(level, "Level {0} State '{1}': {2} Decisions ({3})", level, state, datagramDecisionList.Length, datagramDecisionList.ListToString());

                //
                // Enumerate decisions already made
                //
                for (int i = 0; i < datagramDecisionList.Length; i++)
                {
                    ControllerDatagram datagram = datagramDecisionList[i];
                    Int32 levelDecisionMadeAt = decisionSet.CheckDecision(state, datagram, level);

                    if(levelDecisionMadeAt < 0)
                    {
                        Console.WriteLine(level, "Decision {0}: Already made in level {1}", datagram, levelDecisionMadeAt);
                    }
                }

                //
                // Enumerate decisions not yet made
                //
                for (int i = 0; i < datagramDecisionList.Length; i++)
                {
                    ControllerDatagram datagram = datagramDecisionList[i];
                    Int32 levelDecisionMadeAt = decisionSet.CheckDecision(state, datagram, level);

                    if (levelDecisionMadeAt >= 0)
                    {
                        //EnumerateDecisions(level + 1, ControllerState state, DecisionSet decisionSet, StateToDatagramDecisionLookup lookup)
                    }
                }
            }




        }

    }
    */
}
