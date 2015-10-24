// Author(s): Sébastien Lorion
// original version: http://www.wintellect.com/CS/blogs/jlikness/archive/2010/06/10/tips-and-tricks-for-inotifypropertychanged.aspx

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NLight.Core
{
	/// <summary>
	/// Contains extensions for <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public static class INotifyPropertyChangedExtensions
	{
		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <typeparam name="T">The property return type.</typeparam>
		/// <param name="source">The source of the event.</param>
		/// <param name="propertyExpression">The property expression.</param>
		/// <param name="handler">The event handler.</param>
		public static void RaisePropertyChanged<T>(this INotifyPropertyChanged source, Expression<Func<T>> propertyExpression, PropertyChangedEventHandler handler)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));

			handler?.Invoke(source, new PropertyChangedEventArgs(ExtractPropertyName(propertyExpression)));
		}

		/// <summary>
		/// Extracts the name of the property.
		/// </summary>
		/// <typeparam name="T">The property return type.</typeparam>
		/// <param name="propertyExpression">The property expression.</param>
		/// <returns>The name of the property.</returns>
		private static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
		{
			if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));

			var memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
				throw new ArgumentException(Resources.ExceptionMessages.NotMemberAccessExpression, "propertyExpression");

			var property = memberExpression.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException(Resources.ExceptionMessages.ExpressionDoesNotAccessProperty, "propertyExpression");

			return memberExpression.Member.Name;
		}
	}
}