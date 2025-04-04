
using System.Collections.ObjectModel;
namespace 随机抽取学号_Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    ObservableCollection<string> _list = new ObservableCollection<string>();
}
