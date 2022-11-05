namespace BotwConverter;
public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void onBnpButtonPressed(object sender, EventArgs e)
	{
        var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".bnp" } },
                        { DevicePlatform.macOS, new[] {"bnp"} }
                    });

        PickOptions options = new()
        {
            PickerTitle = "Please select a bnp file",
            FileTypes = customFileType
        };
        FilePicker.Default.PickAsync(options);
    }
}

