using System;
using System.Windows;
using System.Windows.Controls;
using LoL_Queue_Assistant.Models;
using LoL_Queue_Assistant.Services;

namespace LoL_Queue_Assistant;

public partial class MainWindow : Window
{
    private AppState state = AppState.Disconnected;
    private ClientDetectionService clientService = new();
    private LeagueEventServices eventService = new();

    public MainWindow()
    {
        InitializeComponent();
        boxtoban.ItemsSource = ChampionData.champions.Select(c => c.name).ToList();
        boxtopick.ItemsSource = ChampionData.champions.Select(c => c.name).ToList();
        Update_view();
    }

    private void Update_view()
    {
        if (state == AppState.Disconnected) {
            DisconnectedView.Visibility = Visibility.Visible;
            ConnectedView.Visibility = Visibility.Collapsed;
        } else {
            DisconnectedView.Visibility = Visibility.Collapsed;
            ConnectedView.Visibility = Visibility.Visible;
        }
    }

    public int Get_champion_id(ComboBox box)
    {
        Champion champ = ChampionData.champions[box.SelectedIndex];
        return champ.id;
    }
    private async void Detect_client(object sender, RoutedEventArgs e)
    {
        try {
            if (clientService.IsClientOpen()) {
                await eventService.connect(Get_champion_id(boxtoban), Get_champion_id(boxtopick));
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