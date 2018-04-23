using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace CollectionViewSupport {
    public class ViewModel {
        ICollectionView data;
        public ICollectionView Data {
            get {
                if(data == null) {
                    List<TestData> list = new List<TestData>();
                    for(int i = 0; i < 100; i++) {
                        list.Add(new TestData() { Number1 = i, Number2 = i * 10, Text1 = "row " + i, Text2 = "ROW " + i });
                    }
                    data = new ListCollectionView(list);
                }
                return data;
            }
        }
        List<FilterItem> filters;
        public List<FilterItem> Filters {
            get {
                if(filters == null) {
                    filters = new List<FilterItem>() {
                        new FilterItem() { Caption = "All items" },
                        new FilterItem() { Caption = "Even items", Filter = (item) => { return ((TestData)item).Number1 % 2 == 0; } },
                        new FilterItem() { Caption = "Odd items", Filter = (item) => { return ((TestData)item).Number1 % 2 == 1; } }
                    };
                }
                return filters;
            }
        }
    }
    public class FilterItem {
        public string Caption { get; set; }
        public Predicate<object> Filter { get; set; }
    }
    public class TestData {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }
}
