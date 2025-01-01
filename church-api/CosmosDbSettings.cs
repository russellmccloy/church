namespace church_api
{
    public class CosmosDbSettings
    {
        public string CosmosDbConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string PartitionKeyPath { get; set; }
    }
}
