using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models;

public class GoogleSheetsService
{
    private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    private readonly string ApplicationName = "prayercards";

    private readonly string
        SpreadsheetId = "1dxKkT5T9WZVUPhFjBahceJo8TXnce-1anxOK_aEKl9g"; // Replace with your Google Sheet ID

    private readonly string SheetName = "Prayer Warriors"; // Adjust as needed
    private readonly string CredentialsPath = "credentials.json"; // Ensure this is correct

    public async Task<List<Adult>> ReadDataAsync()
    {
      GoogleCredential credential;
        using (var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        string range = $"{SheetName}!A:D"; 
        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

        ValueRange response = await request.ExecuteAsync();
        IList<IList<object>> values = response.Values;

        var adults = new List<Adult>();

        if (values != null && values.Count > 1)
        {
            foreach (var row in values.Skip(1)) // Skip the header row
            {
                if (row.Count < 3) continue; // Ensure row has enough columns

                // Parse the adult ID from the row and check if the parsing is successful
                int adultId;
                bool isParsed = int.TryParse(row[0].ToString(), out adultId);

                // Find if the adult already exists in the list (by AdultId) if parsing is successful
                var adult = isParsed ? adults.FirstOrDefault(a => a.AdultId == adultId) : null;

                if (adult == null)
                {
                    // Create a new adult and add them to the list if not found
                    adult = new Adult
                    {
                        AdultId = adultId, // Use the parsed adult ID
                        AdultName = row[1].ToString()
                    };
                    adults.Add(adult);
                }

                // Add youth under the existing adult
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
