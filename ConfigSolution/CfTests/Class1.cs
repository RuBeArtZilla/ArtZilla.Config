using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtZilla.Config.Tests.TestConfigurations;

namespace ArtZilla.Config.Tests {
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
			Waifu = x.Waifu;
			_guid = x._guid;
		}

		private void OnProperty(string v) => throw new NotImplementedException();

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

		private int _value;
		private ShoujoState _waifu = new ShoujoState(Girls.Homura, true);
	}
}
