/**
 * Modbus TCP Master Example CSharp
 * ----------------------------------------------------------------------------
 * Creates a simple Modbus TCP Master application that polls specific Modbus 
 * registers. 
 *
 * More information https://github.com/chipkin/ModbusTCPMasterExampleCSharp
 * 
 * Created by: Steven Smethurst 
 * Created on: June 16, 2019 
 * Last updated: July 16, 2019 
 */

using Chipkin;
using ModbusExample;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ModbusTCPMasterExampleCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            ModbusMaster modbusServer = new ModbusMaster();
            modbusServer.Run();
        }

        unsafe class ModbusMaster
        {
            // Version 
            const string APPLICATION_VERSION = "0.0.1";

            // TCP 
            Socket tcpListener;
            Socket tcpClient;

            // Configuration Options 
            public const byte SETTING_MODBUS_SERVER_SLAVE_ADDRESS = 0;
            public const ushort SETTING_MODBUS_SERVER_TCP_PORT = 502;
            // public Byte[] SETTING_MODBUS_SERVER_TCP_IP = new Byte[] { 178, 128, 239, 15 };
            public Byte[] SETTING_MODBUS_SERVER_TCP_IP = new Byte[] { 192, 168, 1, 26 };

            const System.Byte SETTING_MODBUS_CLIENT_DEVICE_ID = 255;
            const ushort SETTING_MODBUS_CLIENT_ADDRESS = 1;
            const ushort SETTING_MODBUS_CLIENT_LENGTH = 3;

            public void Run()
            {
                // Prints the version of the application and the CAS BACnet stack. 
                Console.WriteLine("Starting Modbus TCP Master Example  version {0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("https://github.com/chipkin/ModbusTCPMasterExampleCSharp");
                Console.WriteLine("FYI: CAS Modbus Stack version: {0}.{1}.{2}.{3}",
                    CASModbusAdapter.GetAPIMajorVersion(),
                    CASModbusAdapter.GetAPIMinorVersion(),
                    CASModbusAdapter.GetAPIPatchVersion(),
                    CASModbusAdapter.GetAPIBuildVersion());


                // Set up the API and callbacks.
                uint returnCode = CASModbusAdapter.Init(CASModbusAdapter.TYPE_TCP, SendMessage, RecvMessage, CurrentTime);
                if (returnCode != CASModbusAdapter.STATUS_SUCCESS)
                {
                    Console.WriteLine("Error: Could not init the Modbus Stack, returnCode={0}", returnCode);
                    return;
                }

                // All done with the Modbus setup. 
                Console.WriteLine("FYI: CAS Modbus Stack Setup, successfuly");


                // Configure the TCP Listen class. 
                // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/synchronous-server-socket-example 
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, SETTING_MODBUS_SERVER_TCP_PORT);
                this.tcpListener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Program loop. 
                while (true)
                {
                    try
                    {
                        // Check for user input 
                        DoUserInput();

                        // Run the Modbus loop proccessing incoming messages.
                        CASModbusAdapter.Loop();

                        // Give some time to other applications. 
                        System.Threading.Thread.Sleep(1);

                        // If we are connected to a Modbus Slave device. proccess incoming message. 
                        if (this.tcpClient != null && this.tcpClient.Connected)
                        {
                            // Finally flush the buffer
                            CASModbusAdapter.Flush();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        if (this.tcpClient != null && this.tcpClient.Connected)
                        {
                            this.tcpClient.Disconnect(true);
                        }
                    }
                } // Main program loop 
            }

            public bool SendMessage(System.UInt16 connectionId, System.Byte* payload, System.UInt16 payloadSize)
            {
                if (this.tcpClient == null || !this.tcpClient.Connected)
                {
                    return false;
                }
                Console.WriteLine("FYI: Sending {0} bytes to {1}", payloadSize, this.tcpClient.RemoteEndPoint.ToString());


                // Copy from the unsafe pointer to a Byte array. 
                byte[] message = new byte[payloadSize];
                Marshal.Copy((IntPtr)payload, message, 0, payloadSize);

                try
                {
                    // Send the message 
                    if (this.tcpClient.Send(message) == payloadSize)
                    {
                        // Message sent 
                        Console.Write("    ");
                        Console.WriteLine(BitConverter.ToString(message).Replace("-", " ")); // Convert bytes to HEX string. 
                        return true;
                    }
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    if (e.ErrorCode == 10054)
                    {
                        // Client disconnected. This is normal. 
                        return false;
                    }
                    Console.WriteLine(e.ToString());
                    return false;
                }
                // Could not send message for some reason. 
                return false;
            }
            public int RecvMessage(System.UInt16* connectionId, System.Byte* payload, System.UInt16 maxPayloadSize)
            {
                if (this.tcpClient == null || !this.tcpClient.Connected)
                {
                    return 0;
                }

                // Data buffer for incoming data.  
                byte[] bytes = new Byte[maxPayloadSize];

                // Try to get the data from the socket. 
                try
                {
                    if (this.tcpClient.Available <= 0)
                    {
                        return 0;
                    }
                    // An incoming connection needs to be processed.  
                    int bytesRec = this.tcpClient.Receive(bytes);
                    if (bytesRec <= 0)
                    {
                        return 0;
                    }


                    // Copy from the unsafe pointer to a Byte array. 
                    byte[] message = new byte[bytesRec];
                    Marshal.Copy(bytes, 0, (IntPtr)payload, bytesRec);

                    // Debug Show the data on the console.  
                    Console.WriteLine("FYI: Recived {0} bytes from {1}", bytesRec, this.tcpClient.RemoteEndPoint.ToString());
                    Console.Write("    ");
                    Console.WriteLine(BitConverter.ToString(bytes).Replace("-", " ").Substring(0, bytesRec * 3)); // Convert bytes to HEX string. 
                    return bytesRec;
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    if (e.ErrorCode == 10054)
                    {
                        // Client disconnected. This is normal. 
                        return 0;
                    }
                    Console.WriteLine(e.ToString());
                    return 0;
                }
            }
            public ulong CurrentTime()
            {
                // https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
                return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            public void PrintHelp()
            {
                Console.WriteLine("FYI: Modbus Stack version: {0}.{1}.{2}.{3}",
                    CASModbusAdapter.GetAPIMajorVersion(),
                    CASModbusAdapter.GetAPIMinorVersion(),
                    CASModbusAdapter.GetAPIPatchVersion(),
                    CASModbusAdapter.GetAPIBuildVersion());

                Console.WriteLine("Help:");
                Console.WriteLine("   q   - Quit");
                Console.WriteLine("   w   - Write values to a Modbus Slave.");
                Console.WriteLine("   r   - Read values from a Modbus Slave.");
                Console.WriteLine("\n");
            }

            // https://stackoverflow.com/questions/28595903/copy-from-intptr-16-bit-array-to-managed-ushort
            public static void CopyBytesIntoPointer(IntPtr source, ushort[] destination, int startIndex, int length)
            {
                unsafe
                {
                    var sourcePtr = (ushort*)source;
                    for (int i = startIndex; i < startIndex + length; ++i)
                    {
                        destination[i] = *sourcePtr++;
                    }
                }
            }
            public static void CopyBytesIntoPointer(ushort[] source, IntPtr destination)
            {
                unsafe
                {
                    var sourcePtr = (ushort*)destination;
                    foreach (System.UInt16 value in source)
                    {
                        *sourcePtr++ = value;
                    }
                }
            }

            private void DoUserInput()
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    Console.WriteLine("");
                    Console.WriteLine("FYI: Key {0} pressed. ", key.Key);

                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            Environment.Exit(0);
                            break;
                        case ConsoleKey.R:
                            ReadExample();
                            break;
                        case ConsoleKey.W:
                            WriteExample();
                            break;
                        default:
                            this.PrintHelp();
                            break;
                    }
                }
            }

            public void WriteExample()
            {
                Console.WriteLine("FYI: WriteExample");

                // Create the connection 
                IPAddress iPAddress = new IPAddress(SETTING_MODBUS_SERVER_TCP_IP);
                this.tcpClient = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Attempt to connect to the Modbus slave. 
                this.tcpClient.Connect(new IPEndPoint(iPAddress, SETTING_MODBUS_SERVER_TCP_PORT));
                // We are connected. 
                Console.WriteLine("FYI: Connected to IP address=[{0}]", this.tcpClient.RemoteEndPoint.ToString());

                System.UInt16[] data = new System.UInt16[SETTING_MODBUS_CLIENT_LENGTH];
                data[0] = 99;
                data[1] = 150;
                data[2] = 306;

                System.Byte exceptionCode;
                unsafe
                {
                    // Set some unmanaged memory up. 
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(SETTING_MODBUS_CLIENT_LENGTH);
                    CopyBytesIntoPointer(data, unmanagedPointer);


                    // Send the message 
                    Console.WriteLine("FYI: Sending WriteRegisters message");
                    uint resultWriteRegisters = CASModbusAdapter.WriteRegisters(1, SETTING_MODBUS_CLIENT_DEVICE_ID, CASModbusAdapter.FUNCTION_10_FORCE_MULTIPLE_REGISTERS, SETTING_MODBUS_CLIENT_ADDRESS, unmanagedPointer, (ushort)(data.Length * sizeof(System.UInt16)), &exceptionCode);
                    Marshal.FreeHGlobal(unmanagedPointer);
                    this.tcpClient.Close();

                    // Print the results. 
                    if (resultWriteRegisters == CASModbusAdapter.STATUS_SUCCESS)
                    {
                        Console.WriteLine("Write was successful.");
                    }
                    else if (resultWriteRegisters == CASModbusAdapter.STATUS_ERROR_MODBUS_EXCEPTION)
                    {
                        Console.WriteLine("Modbus.Exception={0}", exceptionCode);
                    }
                    else
                    {
                        Console.WriteLine("ModbusStack.Error={0}", resultWriteRegisters);
                    }
                }
            }
            public void ReadExample()
            {
                Console.WriteLine("FYI: ReadExample");

                // Create the connection 
                IPAddress iPAddress = new IPAddress(SETTING_MODBUS_SERVER_TCP_IP);
                this.tcpClient = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Attempt to connect to the Modbus slave. 
                this.tcpClient.Connect(new IPEndPoint(iPAddress, SETTING_MODBUS_SERVER_TCP_PORT));
                // We are connected. 
                Console.WriteLine("FYI: Connected to IP address=[{0}]", this.tcpClient.RemoteEndPoint.ToString());

                System.UInt16[] data = new System.UInt16[SETTING_MODBUS_CLIENT_LENGTH];
                System.Byte exceptionCode;
                unsafe
                {
                    // Set some unmanaged memory up. 
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(SETTING_MODBUS_CLIENT_LENGTH);

                    // Send the message 
                    Console.WriteLine("FYI: Sending ReadRegisters message");
                    uint resultReadRegisters = CASModbusAdapter.ReadRegisters(1, SETTING_MODBUS_CLIENT_DEVICE_ID, CASModbusAdapter.FUNCTION_03_READ_HOLDING_REGISTERS, SETTING_MODBUS_CLIENT_ADDRESS, SETTING_MODBUS_CLIENT_LENGTH, unmanagedPointer, (ushort)(data.Length * sizeof(System.UInt16)), &exceptionCode);

                    // 1 success 
                    if (resultReadRegisters == CASModbusAdapter.STATUS_SUCCESS)
                    {
                        // Extract the data from the unmannged memory into a managed buffer. 
                        CopyBytesIntoPointer(unmanagedPointer, data, 0, SETTING_MODBUS_CLIENT_LENGTH);
                        Marshal.FreeHGlobal(unmanagedPointer);
                        this.tcpClient.Close();

                        // Print the data to the screen. 
                        Console.Write("FYI: Data: ");
                        foreach (System.UInt16 value in data)
                        {
                            Console.Write(value);
                            Console.Write(", ");
                        }
                        Console.WriteLine("");
                    }
                    else if (resultReadRegisters == CASModbusAdapter.STATUS_ERROR_MODBUS_EXCEPTION)
                    {
                        Console.WriteLine("Modbus.Exception={0}", exceptionCode);
                    }
                    else
                    {
                        Console.WriteLine("ModbusStack.Error={0}", resultReadRegisters);
                    }
                }
            }
        }
    }
}
