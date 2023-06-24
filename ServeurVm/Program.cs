using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;



class Program
{
    static void Main()
    {
        int Nb = 0;
        while (true)
        {
            string getid=GetId();
            Console.Clear();
            Console.WriteLine(getid);
            Console.WriteLine(Nb);
            if (Nb++ > 30)
                {
                    Nb = 0;
                    powershell("Get-VM | Select-Object Name, State");
                }
            if (getid == "0")
                Thread.Sleep(1000);
            else
            {
                Nb = 800;
                Thread.Sleep(50);
            }

        }
    }

    private static void powershell(string arguments)
    {
        string command = "powershell";

        ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

    }

    private static string GetId()
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage response = client.GetAsync("https://tools.alize-sas.fr/serveur/GetId.php").Result;
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return responseBody;
    }

    private static string httpsenv(string jvar)
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage response = client.GetAsync("https://tools.alize-sas.fr/serveur/GetVm.php" + jvar).Result;
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return responseBody;
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            string[] properties = e.Data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (properties.Length >= 2)
            {
                string vmName = properties[0];
                string vmState = properties[1];
                if ((vmState != "-----") && (vmState != "State"))

                {
                    Console.WriteLine("ETAT: {1}  ,  NOM: {0}", vmName, vmState);

                    string responseBody = httpsenv("?name=" + vmName + "&etat=" + vmState);

                    Console.WriteLine(responseBody);

                    if (responseBody == "GO")
                    {
                        powershell("start-vm " + vmName);
                    }
                }


            }
        }
    }


    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Console.WriteLine("Erreur : " + e.Data);
        }
    }
}
