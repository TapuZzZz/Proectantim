using System;
using System.IO.Ports;

class Program
{
    private const string SenderPortName = "/tmp/com1";
    private const int BaudRate = 9600;
    private const string ExitCommand = "exit";

    private static void Main()
    {
        using SerialPort port = new SerialPort(SenderPortName, BaudRate, Parity.None, 8, StopBits.One);
        port.Open();

        Console.WriteLine($"Sender connected to {SenderPortName}");
        Console.WriteLine($"Type a message and press Enter. Type {ExitCommand} to quit.");

        SendLoop(port);

        Console.WriteLine("Sender stopped.");
    }

    private static void SendLoop(SerialPort port)
    {
        while (true)
        {
            string message = ReadUserInput();
            port.WriteLine(message);

            if (message == ExitCommand) return;
        }
    }

    private static string ReadUserInput()
    {
        Console.Write("> ");
        return Console.ReadLine() ?? ExitCommand;
    }
}