using System.Text.Json;
namespace AirportTicketBookingSystem.Persistence;
public sealed class JsonFileRepository<T>(string directory, string fileName)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web) { WriteIndented = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    private string Path => System.IO.Path.Combine(directory, fileName);
    public async Task<List<T>> ReadAsync(CancellationToken ct = default) { Directory.CreateDirectory(directory); if (!File.Exists(Path)) { await File.WriteAllTextAsync(Path, "[]", ct); return []; } try { return JsonSerializer.Deserialize<List<T>>(await File.ReadAllTextAsync(Path, ct), options) ?? []; } catch (JsonException ex) { throw new InvalidDataException($"Malformed data file {fileName}.", ex); } }
    public async Task WriteAsync(List<T> items, CancellationToken ct = default) { Directory.CreateDirectory(directory); await gate.WaitAsync(ct); try { var temp = Path + ".tmp"; await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(items, options), ct); File.Move(temp, Path, true); } finally { gate.Release(); } }
}
