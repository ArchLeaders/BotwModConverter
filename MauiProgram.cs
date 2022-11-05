namespace BotwConverter;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("HyliaSerifBeta-Regular.otf", "hylia");

			});

		return builder.Build();
	}
}
