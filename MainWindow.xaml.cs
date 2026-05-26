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
namespace LoL_Queue_Assistant;
public partial class MainWindow : Window
{
    public MainWindow() {
        InitializeComponent();
    }

    private ClientDetectionService clientService = new ClientDetectionService();
    void Detect_client(object sender, RoutedEventArgs e)
    {
        if (clientService.IsClientOpen())
            MessageBox.Show("open");
        else
            MessageBox.Show("close");
    }
}