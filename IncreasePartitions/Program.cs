using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;

namespace IncreasePartitions
{
    internal static class Program
    {
        private const string DefaultEventHubName = "EHLab1Hub";

        private static async Task Main(string[] args)
        {
            string? subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            string? resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP");
            string? namespaceName = Environment.GetEnvironmentVariable("AZURE_EVENTHUB_NAMESPACE");
            string eventHubName = Environment.GetEnvironmentVariable("AZURE_EVENTHUB_NAME") ?? DefaultEventHubName;
            string? managedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

            if (string.IsNullOrWhiteSpace(subscriptionId) ||
                string.IsNullOrWhiteSpace(resourceGroupName) ||
                string.IsNullOrWhiteSpace(namespaceName))
            {
                Console.WriteLine("Set AZURE_SUBSCRIPTION_ID, AZURE_RESOURCE_GROUP, and AZURE_EVENTHUB_NAMESPACE to run this sample.");
                Console.WriteLine("Optional: AZURE_EVENTHUB_NAME (defaults to EHLab1Hub), AZURE_CLIENT_ID for user-assigned managed identity.");
                return;
            }

            DefaultAzureCredentialOptions credentialOptions = new();
            if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
            }

            ArmClient armClient = new(new DefaultAzureCredential(credentialOptions), subscriptionId);
            ResourceIdentifier eventHubResourceId = EventHubResource.CreateResourceIdentifier(
                subscriptionId,
                resourceGroupName,
                namespaceName,
                eventHubName);

            EventHubResource eventHub = armClient.GetEventHubResource(eventHubResourceId);
            Response<EventHubResource> eventHubResponse = await eventHub.GetAsync();
            EventHubData eventHubData = eventHubResponse.Value.Data;

            //Display the number of partitions
            Console.WriteLine("Event hub has {0} partitions", eventHubData.PartitionCount);

            //Double the number of partitions
            if (!eventHubData.PartitionCount.HasValue)
            {
                Console.WriteLine("Unable to determine the current partition count for the target Event Hub.");
                return;
            }

            long updatedPartitionCount = eventHubData.PartitionCount.Value * 2;

            //Update the Event hub
            eventHubData.PartitionCount = updatedPartitionCount;
            await eventHubResponse.Value.UpdateAsync(WaitUntil.Completed, eventHubData);

            Console.WriteLine("Event hub now has {0} partitions", updatedPartitionCount);
        }
    }
}
