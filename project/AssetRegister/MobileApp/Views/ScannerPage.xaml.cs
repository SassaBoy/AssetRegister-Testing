using ZXing.Net.Maui;

namespace MobileApp.Views;

// Shows the camera. When a code is read it closes itself and hands the code back
// through the "onScanned" action that the calling page gave us.
public partial class ScannerPage : ContentPage
{
    private readonly Action<string> _onScanned;
    private bool _finished;   // so one label is not reported many times

    public ScannerPage(Action<string> onScanned)
    {
        InitializeComponent();
        _onScanned = onScanned;

        // The three barcode types we need to support
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Code128 | BarcodeFormat.Code39 | BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ask for the camera permission
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Camera", "Camera permission is needed to scan. You can still type the code.", "OK");
        }
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_finished) return;

        var code = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(code)) return;

        _finished = true;
        Finish(code);
    }

    private async void OnTypeCodeClicked(object? sender, EventArgs e)
    {
        string? code = await DisplayPromptAsync("Type code", "Enter the barcode number");
        if (!string.IsNullOrWhiteSpace(code) && !_finished)
        {
            _finished = true;
            Finish(code);
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void Finish(string code)
    {
        // The camera callback is not on the main thread, so jump to it before touching the screen
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { Vibration.Default.Vibrate(150); } catch { /* some phones have no vibration, ignore */ }

            await Navigation.PopModalAsync();
            _onScanned(code);
        });
    }
}
