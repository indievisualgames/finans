using System;
//using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
public class ServerDateTime
{
    public static DateTime GetFastestNISTDate()
    {
        //CultureInfo culture = new CultureInfo("en-US");
        try
        {
            var client = new TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                // Typical line contains: "56971 24-11-03 21:11:07 50 0 0 478.2 UTC(NIST) *"
                // Extract the yy-MM-dd token safely
                var parts = response.Split(' ');
                foreach (var p in parts)
                {
                    if (p.Length == 8 && p[2] == '-' && p[5] == '-')
                    {
                        if (DateTime.TryParseExact(p, "yy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
                        {
                            return dateOnly;
                        }
                    }
                }
                return DateTime.Now;
            }
        }
        catch
        {
            // Ignore exception and try the next server
            return DateTime.Now;
        }
    }

}

