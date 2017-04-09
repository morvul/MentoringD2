using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace Sample02
{
	[TestClass]
	public class EvaluateExpressionTest
	{
		[TestMethod]
		public void Compile()
		{
			Expression<Func<int, int>> exp1 = (a) => 1 + a;
			Console.WriteLine("{0} : {1}", exp1, exp1.Compile().Invoke(3));

			Expression<Action<int>> exp2 = (a) => Console.WriteLine(a);
			Console.WriteLine("{0} : ", exp2);
			exp2.Compile().Invoke(3);
		}
	}
}
