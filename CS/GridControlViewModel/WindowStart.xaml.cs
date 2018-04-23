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
using System.Windows.Shapes;
using System.Collections;

namespace GridControlViewModel {
    /// <summary>
    /// Interaction logic for WindowStart.xaml
    /// </summary>
    public partial class WindowStart : Window {
        public WindowStart() {
            InitializeComponent();
        }
        public static IList CreateList() {
            List<TestData> list = new List<TestData>();
            for(int i = 0; i < 100; i++) {
                list.Add(new TestData() {
                    Number1 = i,
                    Number2 = i * 10,
                    Text1 = "row " + i,
                    Text2 = "ROW " + i
                });
            }
            return list;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            new Window1().ShowDialog();
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            new Window2().ShowDialog();
        }

        private void button3_Click(object sender, RoutedEventArgs e) {
            new Window3().ShowDialog();
        }
    }
    public class TestData {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }
}
