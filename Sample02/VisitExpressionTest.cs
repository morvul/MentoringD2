using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace Sample02
{
	[TestClass]
	public class VisitExpressionTest
	{
		public class TraceExpressionVisitor : ExpressionVisitor
		{
			public int indent = 0;

			public override Expression Visit(Expression node)
			{
				if (node == null)
					return base.Visit(node);

				Console.WriteLine("{0}{1} - {2}", new String(' ', indent * 4), 
					node.NodeType, node.GetType());

				indent++;
				Expression result = base.Visit(node);
				indent--;

				return result;
			}
		}

		[TestMethod]
		public void Visit()
		{
			Expression<Func<int, int>> exp1 = (a) => 1 + 3 * a;

			new TraceExpressionVisitor().Visit(exp1);
		}
	}
}
