using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Sample02_DSL
{
	public static class DialogFactory
	{
		public static DialogBuilder<T> CreateDialog<T>()
		{
			return new DialogBuilder<T>();
		}
		
		public static DialogInputField<T> CreateField<T>(Expression<Func<T, object>> propertyExpression)
		{
			PropertyInfo property = null;

			if (propertyExpression.Body is MemberExpression)
			{
				property = ((MemberExpression) propertyExpression.Body).Member as PropertyInfo;
			}

			return new DialogInputField<T>(property);
		}
	}
}