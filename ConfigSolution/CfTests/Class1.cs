using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		private void OnProperty(string v) => throw new NotImplementedException();

		private Guid _guid = Guid.NewGuid();

		private int _value;
	}
}
