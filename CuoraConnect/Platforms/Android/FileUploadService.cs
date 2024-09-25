using System;
using System.IO;
using System.Xml.Linq;
using Android.App;
using Android.OS;
using CuoraConnect.Platforms.Android;
using CuoraConnect.Services;
using Application = Android.App.Application;

[assembly: Dependency(typeof(FileUploadService))]
namespace CuoraConnect.Platforms.Android
{
    public class FileUploadService : IFileUploadService
    {
        private string _filePath;

        public FileUploadService()
        {
            // Inicializa o caminho do arquivo
            _filePath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_default.xml");
        }

        public string SaveXmlToFile()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_default.xml");
            }

            // Verifica se o arquivo já existe
            if (!File.Exists(_filePath))
            {
                var xmlContent = GenerateXml();
                File.WriteAllText(_filePath, xmlContent);
                System.Diagnostics.Debug.WriteLine($"XML salvo em: {_filePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Arquivo já existe em: {_filePath}");
            }

            return _filePath;
        }

        public string GenerateXml()
        {
            var config = new XElement("Config",
                new XElement("Setting1", "Value1"),
                new XElement("Setting2", "Value2"),
                new XElement("Setting3", "Value3")
            );

            return config.ToString();
        }
    }

}
