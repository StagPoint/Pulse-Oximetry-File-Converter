namespace OximetryConverter.Exporters;

public class ExporterRegistry
{
	public static List<IOximetryExporter> RegisteredLoaders = new();

	static ExporterRegistry()
	{
		RegisteredLoaders.Add( new MedViewExporter() );
	}
}
