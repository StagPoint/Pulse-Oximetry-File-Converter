namespace cpap_app.Importers;

public interface IOximetryImporter
{
	public string FriendlyName         { get; }
	public string Source               { get; }
	public string FileExtension        { get; }
	public string FileTypeFilters      { get; }
	public string FilenameMatchPattern { get; }

	public Session? Load( string filename, Stream stream );
}
