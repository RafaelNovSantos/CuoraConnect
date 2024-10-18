using SQLite;
using CuoraConnect.Models;
using CuoraConnect.Platforms.Android;
using CuoraConnect.Services;
using Application = Android.App.Application;
using System.Diagnostics;

[assembly: Dependency(typeof(FileExportService))]
namespace CuoraConnect.Platforms.Android
{
    public class FileExportService : IFileExportService
    {
        private string _filePathExport;

        public FileExportService()
        {
            // Inicializa o caminho do arquivo
            _filePathExport = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_export.xml");
        }

        public string SaveXmlToFile()
        {
            if (string.IsNullOrEmpty(_filePathExport))
            {
                _filePathExport = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_export.xml");
            }

            // Verifica se o arquivo já existe
            if (File.Exists(_filePathExport))
            {
                // Apaga o arquivo existente
                File.Delete(_filePathExport);
                System.Diagnostics.Debug.WriteLine($"Arquivo existente apagado: {_filePathExport}");
            }

            return _filePathExport;
        }

        //public string pathDB()
        //{
        //    var dbPath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "networkinfo.db");
        //    return dbPath;
        //}

       
    }
}