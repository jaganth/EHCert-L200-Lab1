using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncreasePartitions
{
    class Program
    {
        static string connectionString = "[REPLACE-WITH-CONNECTION-STRING]";
        static string eventhubName = "EHLab1Hub";
        static void Main(string[] args)
        {
            //Get Event Hub details
            var mgr = NamespaceManager.CreateFromConnectionString(connectionString);
            var ehDesc = mgr.GetEventHub(eventhubName);

            //Display the number of partitions
            Console.WriteLine("Event hub has {0} partitions", ehDesc.PartitionCount);

            //Double the number of partitions
            ehDesc.PartitionCount = ehDesc.PartitionCount * 2;

            //udpate the Event hub
            mgr.UpdateEventHub(ehDesc);
            
        }
    }
}
