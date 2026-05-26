using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LoL_Queue_Assistant.Services;
using System.Data;
using LoL_Queue_Assistant.Models;
using System.Runtime.InteropServices.Java;
using System.Security;
namespace LoL_Queue_Assistant;
public partial class MainWindow : Window
{
    public MainWindow() {
        InitializeComponent();
        Update_view();
    }
    private AppState state = AppState.Disconnected;

    private void Update_view()
    {
        if (state == AppState.Disconnected) {
            DisconnectedView.Visibility = Visibility.Visible;
            ConnectedView.Visibility = Visibility.Collapsed;
        } else if (state == AppState.Connected) {
            DisconnectedView.Visibility = Visibility.Collapsed;
            ConnectedView.Visibility = Visibility.Visible;
        }
        
    }
    private ClientDetectionService clientService = new();
    private LeagueEventServices eventService = new();

    private async void Detect_client(object sender, RoutedEventArgs e)
    {
        try {
            if (clientService.IsClientOpen()) {
                await eventService.connect();
                state = AppState.Connected;
            } else {
                state = AppState.Disconnected;
            }
        } catch (Exception ex) {
            MessageBox.Show(ex.Message);
            state = AppState.Disconnected;
        }

        Update_view();
    }
}