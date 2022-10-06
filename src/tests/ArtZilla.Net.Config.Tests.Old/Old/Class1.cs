using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using ArtZilla.Net.Config.Tests.TestConfigurations;

namespace ArtZilla.Net.Config.Tests; 

public class Class1 {
	public int Aaaa {
		get { return _value; }
		set {
			if (_value == value)
				return;
			_value = value;
			OnProperty(nameof(Aaaa));
		}
	}

	private void Copy(Class1 x) {
		if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
		if (ReferenceEquals(this, x)) return;

		Waifu = x.Waifu;
		_guid = x._guid;
		Items = x.Items;
	}

	private void Testst() {
		Items = new ObservableCollection<Hero>();
	}

	private void OnProperty(string v) 
		=> throw new NotImplementedException();

	private Guid _guid = Guid.NewGuid();

	public ShoujoState Waifu {
		get => _waifu;
		set {
			if (Equals(_waifu, value))
				return;
			_waifu = value;
			OnProperty(nameof(Waifu));
		}
	} 

	[XmlIgnore]
	public IList<Hero> Items {
		get => _items;
		set {
			_items.Clear();
			Debug.WriteLine(value.Count);

			var c = value.Count;
			Debug.WriteLine(c.ToString());

			for (var i = 0; i < c; ++i) {
				_items.Add(value[i]);
			}
		}
	}

	[XmlArray("Items")]
	public Hero[] ItemsArray {
		get => Items.ToArray();
		set => Items = value;
	}

	private int _value;
	private ShoujoState _waifu = new ShoujoState(Girls.Homura, true);
	private ObservableCollection<Hero> _items;
}