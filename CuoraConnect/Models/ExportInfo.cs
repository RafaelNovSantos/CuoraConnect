using SQLite;
using System.Xml.Linq;
namespace CuoraConnect.Models.ExportInfo;
public class ExportInfo
{
    [PrimaryKey]
    public string Id { get; set; }
    public string IP_Address { get; set; }
    public string Default_Gateway { get; set; }
    public string Network_Name { get; set; }

}

public class ExportInfoDatabase
{
    readonly SQLiteConnection _database;

    public ExportInfoDatabase(string dbPath)
    {
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<ExportInfo>();
    }

    public List<ExportInfo> GetExportInfos()
    {
        return _database.Table<ExportInfo>().ToList();
    }

    public int SaveExportInfo(ExportInfo info)
    {
        return _database.Insert(info);
    }
}
