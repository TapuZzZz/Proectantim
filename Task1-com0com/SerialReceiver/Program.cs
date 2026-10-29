using System;
using System.IO.Ports;

class Program
{
    private const string ReceiverPortName = "/tmp/com2";
    private const int BaudRate = 9600;
    private const string ExitCommand = "exit";

    private static void Main()
    {
        using SerialPort port = new SerialPort(ReceiverPortName, BaudRate, Parity.None, 8, StopBits.One);
        port.Open();

        Console.WriteLine($"Receiver listening on {ReceiverPortName}");
        Console.WriteLine("Waiting for messages...");

        ReceiveLoop(port);

        Console.WriteLine("Receiver stopped.");
    }

    private static void ReceiveLoop(SerialPort port)
    {
        while (true)
        {
            string message = port.ReadLine();

            if (message == ExitCommand) return;

            Console.WriteLine($"Received: {message}");
        }
    }
}