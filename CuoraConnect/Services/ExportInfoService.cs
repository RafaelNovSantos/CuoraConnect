using CuoraConnect.Models.ExportInfo;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CuoraConnect.Services
{
    public class ExportInfoService : IExportInfoService
    {
        private readonly SQLiteAsyncConnection _database;

        public ExportInfoService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "exportinfo.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<ExportInfo>().Wait(); // Cria a tabela se não existir
        }

        public async Task<List<ExportInfo>> GetExportInfos()
        {
            return await _database.Table<ExportInfo>().ToListAsync();
        }

        public async Task<ExportInfo> GetExportInfoById(string id)
        {
            return await _database.FindAsync<ExportInfo>(id);
        }

        public async Task<string> SalvarExportInfo(ExportInfo exportInfo)
        {
            // Verifica se o Id já foi definido
            if (!string.IsNullOrEmpty(exportInfo.Id))
            {
                // Tenta encontrar o registro existente
                var existingExportInfo = await _database.FindAsync<ExportInfo>(exportInfo.Id);

                if (existingExportInfo != null)
                {
                    // Atualiza o registro se encontrado
                    await _database.UpdateAsync(exportInfo);
                    Console.WriteLine("Registro atualizado com sucesso.");
                }
                else
                {
                    // Se não encontrar, insere um novo registro
                    await _database.InsertAsync(exportInfo);
                    Console.WriteLine("Novo registro criado com sucesso.");
                }
            }
            else
            {
                // Caso o Id não esteja definido, insere um novo registro
                await _database.InsertAsync(exportInfo);
                Console.WriteLine("Novo registro criado com sucesso.");
            }

            return exportInfo.Id; // Retorna o Id da entidade
        }




        public async Task<int> DeletarExportInfo(string id)
        {
            return await _database.DeleteAsync<ExportInfo>(id);
        }
    }
}
