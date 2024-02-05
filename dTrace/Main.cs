using System;
using System.IO;
using System.IO.Pipes;

class dTrace
{
    static void Main()
    {
        // Set the window title
        Console.Title = "dTrace [4] dnelb";

        // Create a directory for log files
        string logDirectory = @"C:\dnelb_temp\dTrace";
        Directory.CreateDirectory(logDirectory);

        // Generate a unique log file name based on the current timestamp
        string logFileName = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        Console.WriteLine(@"
        .    ...........                                                                                    
::::..  ...::........                                                                               
=-------:.....::............                                                                        
**++=======--:....::::......:::::                                                                   
*###*++++++=====--::.::::::.....::-:::.                                                             
-=+*#%#****+++++=====-:::::::::::::::.:-:::.                                                        
..:-=*##%##*****+++=======--:::::----:::::..::::..                                                  
==:..-=+*##%%##****+++++==+++==--:::::------::...::::..                                             
++++=:.:-=+**#%%###****+++++++++++==-:::::------:.....::::..                                        
++++++=-:.:-=+**#%%%###*****+++++++++++=-::..::-----::....::-::..                                   
=**+++====-:::-=+**#%%%#####****+++++++++++=-:....:-===--:....::---::.                              
 .=**+++=====-::-=++*##%%%%#####*****++++++++++=-::...:--===--:...:--==-:                           
    -+*+++==+++=-::-=+**##%@%%#######****+++++++++++=-:...:-====-=*%@@@@*-                          
      :=**+++==+++=-::-=+**##%@%%########****++++++*++++=--:::-=#@@@@@@@@=                          
        .-***++++++++=-::-=+**##%@@%%#########***+++++****+++=*@@@@@@@@@@-                          
           -****++++++++=-::-=+**##%@@%%#########****++++++++#@@@@@@@@%%#.                          
             -*#***++++++++=:.:-=+***#%@@%%%#########***++==#@@@@@%%%%%%+=-:                        
               :*##***+++***+=-:.:-=+**##%@@%%%%%%#######**+@@@%%%%%%%%*==***-                      
                 .+###***+++**++=-..:-++**##%@@@%%%%%#####+#%%%%%#####++%@@@@*                      
                   .=###****+++***+=:..:=++**##%@@@%%%%%%#=%%########+#@@@@@@*                      
                      -*###***++++**++=:..-=+***##%@@@%%%%+*###*****+%@@@%%%%:                      
                        :*###****+++***++=:.:-=+**###%@@@@#++***++#*%%%%%%%%-                       
                          .+####***+++++**++=-:-=++**###%%@#*++*%@*%%%%%%%#-                        
                             =#####***+==+***++=---=++*****+**#*+#*######*.                         
                               -*####***++==+***++=====+++#@@@@@#+*###**-                           
                                 :+####****+===+***+++=+#@@@@@@@#==*+=:                             
                                   .=#####***+===++**+*@@@@%%%%%-                                   
                                     .=######***+=-=+#@@%%%%%%%+                                    
                                        -*######**+=#%%%%%%%%%=                                     
                                          :+######**%%%######-                                      
                                            .+####+########+.                                       
                                               =##+#####*+:                                         
                                                 -++**+-.                                           
                                                   ..    dTrace [4] dnelb by @jsluganovic 
                                                         v[0.5]> 22/01/2024                                           
                                                                                                    
");
        using (var server = new NamedPipeServerStream("dTracePipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
        {
            Console.WriteLine("Waiting for connection...");
            server.WaitForConnection();
            Console.WriteLine("Client connected. Receiving messages...");

            using (var reader = new StreamReader(server))
            {
                using (StreamWriter writer = new StreamWriter(logFileName))
                while (true)
                {
                    string message = reader.ReadLine();
                    if (message == null || message == "stop")
                    {
                        Console.WriteLine($"{message} was received. ");
                        break;
                    }
                    // Add a timestamp to the message
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                    writer.WriteLine(logMessage);

                    Console.WriteLine("Received message: " + message);
                }
            }

            // Continue listening for more messages
            Main();
        }
    }
}
