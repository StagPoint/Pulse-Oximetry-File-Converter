using cpap_app.Importers;

namespace OximetryConverter.Exporters;

public interface IOximetryExporter
{
	public string FriendlyName   { get; }
	public string FileExtension  { get; }
	public string FilenameFilter { get; }

	public string GetSuggestedFilename( Session session );

	public bool Export( Session data, string exportFullPath );
}

