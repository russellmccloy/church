using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

public interface IGoogleSheetsService
{
    Task<List<Adult>> ReadDataAsync();
}