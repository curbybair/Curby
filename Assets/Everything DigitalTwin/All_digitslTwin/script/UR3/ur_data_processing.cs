/****************************************************************************
MIT License
Copyright(c) 2021 Roman Parak
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*****************************************************************************
Author   : Roman Parak
Email    : Roman.Parak @outlook.com
Github   : https://github.com/rparak
File Name: ur_data_processing.cs
****************************************************************************/

// System
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
// Unity 
using UnityEngine;
using Debug = UnityEngine.Debug;





public class ur_data_processing : MonoBehaviour
{
    public static class GlobalVariables_Main_Control
    {
        public static bool connect, disconnect;
    }

    public static class UR_Stream_Data
    {
        // IP Port Number and IP Address
        public static string ip_address;
        //  Real-time (Read Only)
        public const ushort port_number = 30013;
        // Comunication Speed (ms)
        public static int time_step;
        // Joint Space:
        //  Orientation {J1 .. J6} (rad)
        public static double[] J_Orientation = new double[6];
        // Cartesian Space:
        //  Position {X, Y, Z} (mm)
        public static double[] C_Position = new double[3];
        //  Orientation {Euler Angles} (rad):
        public static double[] C_Orientation = new double[3];
        // Class thread information (is alive or not)
        public static bool is_alive = false;
    }
    public static class UR_Control_Data
    {
        // IP Port Number and IP Address
        public static string ip_address;
        //  Real-time (Read/Write)
        public const ushort port_number = 30003;
        // Comunication Speed (ms)
        public static int time_step;
        // Control Parameters UR3/UR3e:
        public static string aux_command_str;
        public static byte[] command;
        public static bool[] button_pressed = new bool[12];
        public static bool joystick_button_pressed;
        // Class thread information (is alive or not)
        public static bool is_alive = false;
    }

    // Class Stream / Control {Universal Robots TCP/IP}
    private UR_Stream ur_stream_robot;
    private UR_Control ur_ctrl_robot;
    //Dashboard control for both robots
    private UR_Control_Dashboard ur_dashboard_robot_1;
    private UR_Control_Dashboard ur_dashboard_robot_2;
    // Other variables
    private int main_ur3_state = 0;
    private int aux_counter_pressed_btn = 0;

    public UR_Control_Dashboard URDashboardRobot1 => ur_dashboard_robot_1;
    public UR_Control_Dashboard URDashboardRobot2 => ur_dashboard_robot_2;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization {TCP/IP Universal Robots}
        //  Read Data:
        UR_Stream_Data.ip_address = "192.168.1.112";
        //  Communication speed: CB-Series 125 Hz (8 ms), E-Series 500 Hz (2 ms)
        UR_Stream_Data.time_step = 2;
        //  Write Data:
        UR_Control_Data.ip_address = "192.168.1.112";
        //  Communication speed: CB-Series 125 Hz (8 ms), E-Series 500 Hz (2 ms)
        UR_Control_Data.time_step = 2;
        

        // Initialization Stream {Universal Robots TCP/IP}/
        ur_stream_robot = new UR_Stream();
        // Start Control {Universal Robots TCP/IP}
        ur_ctrl_robot = new UR_Control();

        // Dashboard control for Robot 1
        ur_dashboard_robot_1 = gameObject.AddComponent<UR_Control_Dashboard>();
        UR_Control_Data.ip_address = "192.168.1.112"; // IP for Robot 1
        ur_dashboard_robot_1.programName = "test"; // Example program
        //ur_dashboard_robot_1.Start(); // Start Robot 1

        // Dashboard ycontrol for Robot 2
        ur_dashboard_robot_2 = gameObject.AddComponent<UR_Control_Dashboard>();
        UR_Control_Data.ip_address = "192.168.1.120"; // IP for Robot 2
        ur_dashboard_robot_2.programName = "robottestnew"; // Example program
        //ur_dashboard_robot_2.Start(); // Start Robot 2
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (main_ur3_state)
        {
            case 0:
                {
                    // ------------------------ Wait State {Disconnect State} ------------------------//

                    if (GlobalVariables_Main_Control.connect == true)
                    {
                        // Start Stream {Universal Robots TCP/IP}
                        ur_stream_robot.Start();
                        // Start Control {Universal Robots TCP/IP}
                        ur_ctrl_robot.Start();

                        // go to connect state
                        main_ur3_state = 1;
                    }
                }
                break;
            case 1:
                {
                    // ------------------------ Data Processing State {Connect State} ------------------------//

                    for (int i = 0; i < UR_Control_Data.button_pressed.Length; i++)
                    {
                        // check the pressed button in joystick control mode
                        if (UR_Control_Data.button_pressed[i] == true)
                        {
                            aux_counter_pressed_btn++;
                        }
                    }

                    // at least one button pressed
                    if (aux_counter_pressed_btn > 0)
                    {
                        // start move -> speed control
                        UR_Control_Data.joystick_button_pressed = true;
                    }
                    else
                    {
                        // stop move -> speed control
                        UR_Control_Data.joystick_button_pressed = false;
                    }

                    // null auxiliary variable
                    aux_counter_pressed_btn = 0;

                    if (GlobalVariables_Main_Control.disconnect == true)
                    {
                        // Stop threading block {TCP/Ip -> read data}
                        if (UR_Stream_Data.is_alive == true)
                        {
                            ur_stream_robot.Stop();
                        }
                        // Stop threading block {TCP/Ip  -> write data}
                        if (UR_Control_Data.is_alive == true)
                        {
                            ur_ctrl_robot.Stop();
                        }
                        if (UR_Stream_Data.is_alive == false && UR_Control_Data.is_alive == false)
                        {
                            // go to initialization state {wait state -> disconnect state}
                            main_ur3_state = 0;
                        }
                    }
                }
                break;
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            // Check if objects are not null before calling Destroy
            if (ur_stream_robot != null)
            {
                ur_stream_robot.Destroy();
            }

            if (ur_ctrl_robot != null)
            {
                ur_ctrl_robot.Destroy();
            }

            if (ur_dashboard_robot_1 != null)
            {
                ur_dashboard_robot_1.Destroy();
            }

            if (ur_dashboard_robot_2 != null)
            {
                ur_dashboard_robot_2.Destroy();
            }

            // Finally, destroy the current MonoBehaviour (this script)
            Destroy(this);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    class UR_Stream
    {
        // Initialization of Class variables
        //  Thread
        private Thread robot_thread = null;
        private bool exit_thread = false;
        //  TCP/IP Communication
        private TcpClient tcp_client = new TcpClient();
        private NetworkStream network_stream = null;
        //  Packet Buffer (Read)
        private byte[] packet = new byte[1116];

        // Offset:
        //  Size of first packet in bytes (Integer)
        private const byte first_packet_size = 4;
        //  Size of other packets in bytes (Double)
        private const byte offset = 8;

        // Total message length in bytes
        private const UInt32 total_msg_length = 3288596480;

        

        public void UR_Stream_Thread()
        {
            try
            {
                if (tcp_client.Connected == false)
                {
                    // Connect to controller -> if the controller is disconnected
                    tcp_client.Connect(UR_Stream_Data.ip_address, UR_Stream_Data.port_number);
                }

                // Initialization TCP/IP Communication (Stream)
                network_stream = tcp_client.GetStream();

                // Initialization timer
                var t = new Stopwatch();

                while (exit_thread == false)
                {
                    // Get the data from the robot
                    if (network_stream.Read(packet, 0, packet.Length) != 0)
                    {
                        if (BitConverter.ToUInt32(packet, first_packet_size - 4) == total_msg_length)
                        {
                            // t_{0}: Timer start.
                            t.Start();

                            // Reverses the order of elements in a one-dimensional array or part of an array.
                            Array.Reverse(packet);

                            // Note:
                            //  For more information on values 32... 37, etc., see the UR Client Interface document.
                            // Read Joint Values in radians
                            UR_Stream_Data.J_Orientation[0] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (32 * offset));
                            UR_Stream_Data.J_Orientation[1] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (33 * offset));
                            UR_Stream_Data.J_Orientation[2] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (34 * offset));
                            UR_Stream_Data.J_Orientation[3] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (35 * offset));
                            UR_Stream_Data.J_Orientation[4] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (36 * offset));
                            UR_Stream_Data.J_Orientation[5] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (37 * offset));
                            // Read Cartesian (Positon) Values in metres
                            UR_Stream_Data.C_Position[0] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (56 * offset));
                            UR_Stream_Data.C_Position[1] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (57 * offset));
                            UR_Stream_Data.C_Position[2] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (58 * offset));
                            // Read Cartesian (Orientation) Values in metres 
                            UR_Stream_Data.C_Orientation[0] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (59 * offset));
                            UR_Stream_Data.C_Orientation[1] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (60 * offset));
                            UR_Stream_Data.C_Orientation[2] = BitConverter.ToDouble(packet, packet.Length - first_packet_size - (61 * offset));

                            // t_{1}: Timer stop.
                            t.Stop();

                            // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                            if (t.ElapsedMilliseconds < UR_Stream_Data.time_step)
                            {
                                Thread.Sleep(UR_Stream_Data.time_step - (int)t.ElapsedMilliseconds);
                            }

                            // Reset (Restart) timer.
                            t.Restart();
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.LogException(e);
            }
        }

        public void Start()
        {
            // Start thread
            exit_thread = false;
            // Start a thread and listen to incoming messages
            robot_thread = new Thread(new ThreadStart(UR_Stream_Thread));
            robot_thread.IsBackground = true;
            robot_thread.Start();
            // Thread is active
            UR_Stream_Data.is_alive = true;
        }
        public void Stop()
        {
            exit_thread = true;
            // Stop a thread
            Thread.Sleep(100);
            UR_Stream_Data.is_alive = robot_thread.IsAlive;
            robot_thread.Abort();
        }
        public void Destroy()
        {
            if (tcp_client.Connected == true)
            {
                // Disconnect communication
                network_stream.Dispose();
                tcp_client.Close();
            }
            Thread.Sleep(100);
        }
    }

    class UR_Control
    {
        // Initialization of Class variables
        //  Thread
        private Thread robot_thread = null;
        private bool exit_thread = false;
        //  TCP/IP Communication
        private TcpClient tcp_client = new TcpClient();
        private NetworkStream network_stream = null;

        public void SendURScriptCommand(string command)
        {
            try
            {
                if (!tcp_client.Connected)
                {
                    tcp_client.Connect(UR_Control_Data.ip_address, UR_Control_Data.port_number);
                }

                if (network_stream == null)
                {
                    network_stream = tcp_client.GetStream();
                }

                // Send the URScript command as a string
                byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(command + "\n");
                network_stream.Write(commandBytes, 0, commandBytes.Length);
            }
            catch (SocketException e)
            {
                Debug.LogError($"Socket exception while sending URScript command: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Unexpected error while sending URScript command: {e.Message}");
            }
        }
        public void UR_Control_Thread()
        {
            try
            {
                if (tcp_client.Connected != true)
                {
                    // Connect to controller -> if the controller is disconnected
                    tcp_client.Connect(UR_Control_Data.ip_address, UR_Control_Data.port_number);

                }
                

                // Initialization TCP/IP Communication (Stream)
                network_stream = tcp_client.GetStream();

                // Initialization timer
                var t = new Stopwatch();

                while (exit_thread == false)
                {
                    // t_{0}: Timer start.
                    t.Start();

                    // Note:
                    //  For more information about commands, see the URScript Programming Language document 

                    if (UR_Control_Data.joystick_button_pressed == true)
                    {
                        // Send command (byte) -> speed control of the robot (X,Y,Z and EA{RX, RY, RZ})
                        network_stream.Write(UR_Control_Data.command, 0, UR_Control_Data.command.Length);
                    }

                    // t_{1}: Timer stop.
                    t.Stop();

                    // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                    if (t.ElapsedMilliseconds < UR_Stream_Data.time_step)
                    {
                        Thread.Sleep(UR_Stream_Data.time_step - (int)t.ElapsedMilliseconds);
                    }

                    // Reset (Restart) timer.
                    t.Restart();
                }
            }
            catch (SocketException e)
            {
                Debug.LogException(e);
            }
        }

        public void Start()
        {
            // Start thread/
            exit_thread = false;
            // Start a thread and listen to incoming messages
            string timeoutCommand = "socket_read_line(device, timeout=-1)";
            SendURScriptCommand(timeoutCommand);
            robot_thread = new Thread(new ThreadStart(UR_Control_Thread));
            robot_thread.IsBackground = true;
            robot_thread.Start();
            // Thread is active
            UR_Control_Data.is_alive = true;
        }
        public void Stop()
        {
            exit_thread = true;
            // Stop a thread
            Thread.Sleep(100);
            UR_Control_Data.is_alive = robot_thread.IsAlive;
            robot_thread.Abort();
        }
        public void Destroy()
        {
            if (tcp_client.Connected == true)
            {
                // Disconnect communication
                network_stream.Dispose();
                tcp_client.Close();
            }
            Thread.Sleep(100);
        }
    }
}


public class UR_Control_Dashboard : MonoBehaviour
{
    // TCP/IP Communication for Dashboard Server
    private TcpClient dashboard_client = new TcpClient();
    private NetworkStream dashboard_stream = null;

    private bool exit_thread = false;
    public string programName;

    

    // Method to send a command to the Dashboard Server
    public void SendDashboardCommand(string command)
    {
        try
        {
            if (dashboard_client.Connected == false)
            {
                // Connect to the robot's Dashboard Server on port 29999
                dashboard_client.Connect(ur_data_processing.UR_Control_Data.ip_address, 29999);
            }

            // Send the command
            dashboard_stream = dashboard_client.GetStream();
            byte[] command_bytes = System.Text.Encoding.ASCII.GetBytes(command + "\n");
            dashboard_stream.Write(command_bytes, 0, command_bytes.Length);

            // Read the response (optional, can be omitted if you don't need the response)
            byte[] response = new byte[256];
            dashboard_stream.Read(response, 0, response.Length);
            string response_str = System.Text.Encoding.ASCII.GetString(response);
            Debug.Log("Dashboard Response: " + response_str);
        }
        catch (SocketException e)
        {
            Debug.LogException(e);
        }
    }

    // Method to load and run a program
    public void RunProgram(string programName)
    {
        // Load the program
        SendDashboardCommand("load " + programName + ".urp");
        //Debug.LogWarning("Loaded Program");
        // Start the program
        SendDashboardCommand("robotmode");
        StartCoroutine(PlayProgramAfterDelay());
    }
    public void CheckRobotStatus()
    {
        SendDashboardCommand("robotmode");
        // You should get a response from the robot that indicates if it's ready
        // Adjust the logic sbased on the response to ensure it's ready to run the next program
    }

    private IEnumerator PlayProgramAfterDelay()
    {
        yield return new WaitForSeconds(1);
        SendDashboardCommand("robotmode"); // Adjust delay if necessary
        SendDashboardCommand("play");
        SendDashboardCommand("robotmode");
        
    }

    public void OnRunRobot1()
    {
        //SendDashboardCommand("set operational mode manual");
        SendDashboardCommand("stop");
        SendDashboardCommand("robotmode");
        RunProgram("test");
    }

    public void OnRunRobot2()
    {
        //SendDashboardCommand("set operational mode automatic");
        SendDashboardCommand("stop");
        SendDashboardCommand("robotmode");
        RunProgram("robottestnew");
    }


    public void Start()
    {
        // Start thread or any other control logic
        exit_thread = false;

        // Example to run a program from Unity
        //RunProgram("robotblocktestnew");
    }

    public void Destroy()
    {
        if (dashboard_client.Connected == true)
        {
            // Disconnect communication
            dashboard_stream.Dispose();
            dashboard_client.Close();
        }
        Thread.Sleep(100);
    }
}