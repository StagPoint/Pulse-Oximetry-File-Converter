using System.Text.RegularExpressions;

namespace cpap_app.Importers;

public static class OximetryImporterRegistry
{
	public static List<IOximetryImporter> RegisteredImporters = new();
	
	static OximetryImporterRegistry()
	{
		RegisteredImporters.Add( new ViatomImporterCSV() );
		RegisteredImporters.Add( new ViatomDesktopImporterCSV() );
		RegisteredImporters.Add( new EmayImporterCSV() );
		//RegisteredLoaders.Add( new EdfLoader() );
		//RegisteredLoaders.Add( new ViatomBinaryImporter() );
	}

	public static IOximetryImporter? GetImporterForFile( string filename )
	{
		foreach( var importer in RegisteredImporters )
		{
			if( Regex.IsMatch( filename, importer.FilenameMatchPattern ) )
			{
				return importer;
			}
		}

		return null;
	}
}
