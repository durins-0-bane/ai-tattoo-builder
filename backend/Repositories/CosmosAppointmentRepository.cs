using Microsoft.Azure.Cosmos;
using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public class CosmosAppointmentRepository : IAppointmentRepository
{
    private readonly Container _container;

    public CosmosAppointmentRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        var containerName = "Appointments";
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task<Appointment?> GetAppointmentAsync(string id, string artistId)
    {
        try
        {
            ItemResponse<Appointment> response = await _container.ReadItemAsync<Appointment>(id, new PartitionKey(artistId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddAppointmentAsync(Appointment appointment)
    {
        await _container.CreateItemAsync(appointment, new PartitionKey(appointment.ArtistId));
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByArtistAsync(string artistId)
    {
        var query = _container.GetItemQueryIterator<Appointment>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @artistId ORDER BY c.startTime ASC")
                .WithParameter("@artistId", artistId));

        var results = new List<Appointment>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(string customerUserId)
    {
        var query = _container.GetItemQueryIterator<Appointment>(
            new QueryDefinition("SELECT * FROM c WHERE c.customerUserId = @customerUserId ORDER BY c.startTime ASC")
                .WithParameter("@customerUserId", customerUserId));

        var results = new List<Appointment>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}