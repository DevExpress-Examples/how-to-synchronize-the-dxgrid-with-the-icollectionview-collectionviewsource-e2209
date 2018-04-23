using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using DevExpress.Wpf.Grid;
using DevExpress.Data;
using System.Collections.Specialized;
using DevExpress.Wpf.Core;
using System.Collections;
using System.Windows.Threading;
using System.Threading;

namespace GridControlViewModel {
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window {
        ListCollectionView view;
        public Window3() {
            InitializeComponent();
            IList list = WindowStart.CreateList();
            view = new ListCollectionView(list);
            DataContext = view;
            filterComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(ComboBox_SelectionChanged);
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            Dispatcher.BeginInvoke(new ThreadStart(UpdateFilter), DispatcherPriority.Background);
        }

        void UpdateFilter() {
            switch(filterComboBox.SelectedIndex) {
                case 0:
                    view.Filter = null;
                    break;
                case 1:
                    view.Filter = EvenFilter;
                    break;
                case 2:
                    view.Filter = OddFilter;
                    break;
                default:
                    break;
            }
        }
        bool EvenFilter(object obj) {
            TestData testData = (TestData)obj;
            return testData.Number1 % 2 == 0;
        }
        bool OddFilter(object obj) {
            TestData testData = (TestData)obj;
            return testData.Number1 % 2 == 1;
        }
    }
}
