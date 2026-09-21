namespace VetSanJose.Infrastructure.Storage;

public class SupabaseStorageSettings
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = "";
    public string ServiceKey { get; set; } = "";
    public string Bucket { get; set; } = "imagenes";
}
