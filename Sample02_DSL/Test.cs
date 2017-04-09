using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sample02_DSL
{
	public class Model1
	{
		[DisplayName("First Field")]
		public string Q { get; set; }
		public string Q2 { get; set; }
	}


	[TestClass]
	public class Test
	{
		[TestMethod]
		[STAThread]
		public void ShowFluentAPIDialog()
		{
			var model = new Model1() { Q = "2", Q2 = "1" };

			var dialog = DialogFactory.CreateDialog<Model1>()
				.WithTitle("My Dialog")
				.AddFields(
					DialogFactory.CreateField<Model1>(model1 => model1.Q),
					DialogFactory.CreateField<Model1>(model1 => model1.Q2)
					)
				.Build(model);

			var res = dialog.ShowDialog();

			if (res.Value)
			{
				Console.WriteLine("{0}, {1}", model.Q, model.Q2);
			}
		}
	}
}
