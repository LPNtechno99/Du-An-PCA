using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartSimpleToolKit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            showColumnChart();
        }
        private void showColumnChart()
        {
            List<KeyValuePair<string, int>> MyValue = new List<KeyValuePair<string, int>>();
            //MyValue.Add(new KeyValuePair<string, int>("Mahak", 300));
            //MyValue.Add(new KeyValuePair<string, int>("Pihu", 250));
            //MyValue.Add(new KeyValuePair<string, int>("Rahul", 289));
            //MyValue.Add(new KeyValuePair<string, int>("Raj", 256));
            //MyValue.Add(new KeyValuePair<string, int>("Vikas", 140));
            for (int i = 0; i < 50; i++)
            {
                if (i % 5 == 0) MyValue.Add(new KeyValuePair<string, int>("c" + i.ToString(), 250));
                else MyValue.Add(new KeyValuePair<string, int>(i.ToString(), 250));
            }

            LineChart1.DataContext = MyValue;

        }
    }
}
