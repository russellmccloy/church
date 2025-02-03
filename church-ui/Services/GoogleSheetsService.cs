using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

public class GoogleSheetsService : IGoogleSheetsService
{
    private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    private readonly string _applicationName;
    private readonly string _spreadsheetId;
    private readonly string _credentialsJson;

    private readonly string SheetName = "Prayer Warriors"; 

    public GoogleSheetsService(IConfiguration configuration)
    {
        _spreadsheetId = configuration["GoogleSheet:SpreadsheetId"];
        _applicationName = configuration["GoogleSheet:ApplicationName"];
        _credentialsJson = configuration["GoogleSheet:CredentialsJson"];
    }

    public async Task<List<Adult>> ReadDataAsync()
    {
        GoogleCredential credential;
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_credentialsJson)))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });

        string range = $"{SheetName}!A:D"; 
        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(_spreadsheetId, range);

        ValueRange response = await request.ExecuteAsync();
        IList<IList<object>> values = response.Values;

        var adults = new List<Adult>();

        if (values != null && values.Count > 1)
        {
            foreach (var row in values.Skip(1))
            {
                if (row.Count < 3) continue;

                int adultId;
                bool isParsed = int.TryParse(row[0].ToString(), out adultId);
                var adult = isParsed ? adults.FirstOrDefault(a => a.AdultId == adultId) : null;

                if (adult == null)
                {
                    adult = new Adult
                    {
                        AdultId = adultId,
                        AdultName = row[1].ToString()
                    };
                    adults.Add(adult);
                }

                adult.YouthList.Add(new Youth
                {
                    Name = row[2].ToString(),
                    Age = int.TryParse(row[3].ToString(), out int age) ? age : 0
                });
            }
        }

        return adults;
    }
}
