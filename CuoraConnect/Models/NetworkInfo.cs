using SQLite;
namespace CuoraConnect.Models;
public class NetworkInfo
{
    [PrimaryKey]
    public string Id { get; set; }
    public string SSID { get; set; }
    public string Gateway { get; set; }
    public string LocalIP { get; set; }
    public string AvailableIP { get; set; }
    public string Password { get; set; }
    public string SubnetMask { get; set; }
    public int CIDR { get; set; }
    //public List<string> Networks { get; set; }


}

public class NetworkInfoDatabase
{
    readonly SQLiteConnection _database;

    public NetworkInfoDatabase(string dbPath)
    {
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<NetworkInfo>();
    }

    public List<NetworkInfo> GetNetworkInfos()
    {
        return _database.Table<NetworkInfo>().ToList();
    }

    public int SaveNetworkInfo(NetworkInfo info)
    {
        return _database.Insert(info);
    }
}
