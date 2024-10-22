using CuoraConnect.Models.ExportInfo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CuoraConnect.Services
{
    public interface IExportInfoService
    {
        Task<List<ExportInfo>> GetExportInfos();
        Task<ExportInfo> GetExportInfoById(string id);
        Task<string> SalvarExportInfo(ExportInfo exportInfo);
        Task<int> DeletarExportInfo(string id);
    }
}

