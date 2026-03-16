using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;

namespace IncreasePartitions
{
    internal static class Program
    {
        private const string SubscriptionId = "<SUBSCRIPTION_ID>";
        private const string TenantId = "<TENANT_ID>";
        private const string ResourceGroupName = "<RESOURCE_GROUP>";
        private const string NamespaceName = "<EVENT_HUBS_NAMESPACE>";
        private const string EventHubName = "<EVENT_HUB_NAME>";

        private static async Task Main()
        {
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                TenantId = TenantId
            });

            ArmClient armClient = new(credential, SubscriptionId);
            ResourceIdentifier eventHubResourceId = EventHubResource.CreateResourceIdentifier(
                SubscriptionId,
                ResourceGroupName,
                NamespaceName,
                EventHubName);

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
