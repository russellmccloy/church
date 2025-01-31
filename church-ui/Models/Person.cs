using Models;

public class Adult
{
    public int AdultId { get; set; }  // Unique ID for the Adult
    public string AdultName { get; set; }  // Name of the adult
    public List<Youth> YouthList { get; set; }  // List of Youth associated with the Adult

    public Adult()
    {
        YouthList = new List<Youth>();
    }
}