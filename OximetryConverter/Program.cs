using System.Diagnostics;

using cpap_app.Importers;

using OximetryConverter.Exporters;

var exporter = new MedViewExporter();

var files = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.csv" );
if( files.Length == 0 )
{
	Console.WriteLine( "No CSV files found in the current directory" );
}
else
{
	foreach( var filePath in files )
	{
		var baseFilename = Path.GetFileName( filePath );

		Console.WriteLine( $"Looking for importers for '{baseFilename}'" );

		var importer = OximetryImporterRegistry.GetImporterForFile( Path.GetFileName( baseFilename ) );
		if( importer == null )
		{
			Console.WriteLine( $"   No suitable importer found for '{baseFilename}'" );
			continue;
		}

		Console.WriteLine( $"    Attempting to convert '{baseFilename}' using '{importer.FriendlyName}'" );

		using var file = File.OpenRead( filePath );

		var session = importer.Load( filePath, file );
		if( session == null )
		{
			Console.WriteLine( $"    Failed to import '{baseFilename}'" );
			continue;
		}

		if( !Directory.Exists( "Export" ) )
		{
			Console.WriteLine( $"Creating export directory" );
			Directory.CreateDirectory( "Export" );
		}

		var outputFilename = Path.Combine( "Export", exporter.GetSuggestedFilename( session ) );
		if( File.Exists( outputFilename ) )
		{
			Console.WriteLine( $"    File '{baseFilename}' has already been converted" );
			continue;
		}
		
		Console.WriteLine( $"    Exporting '{outputFilename}'" );
		exporter.Export( session, outputFilename );
	}
}

Console.Write( "Press ENTER to exit...");
Console.Read();
