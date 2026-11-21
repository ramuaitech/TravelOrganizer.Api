using TravelOrganizer.Maui.ViewModels;

namespace TravelOrganizer.Maui.Views;

public partial class MainPage : ContentPage
{
    public MainPage(TravelAdviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
